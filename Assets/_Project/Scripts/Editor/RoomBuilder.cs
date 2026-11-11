using System.Collections.Generic;
using UnityEngine;
using static EchoChamber.Editor.SceneBuilderUtil;

namespace EchoChamber.Editor
{
    /// <summary>Builds the control room (basement) with the interactive desk objects, from the prototype layout.</summary>
    public static class RoomBuilder
    {
        public const float DeskY = 0.8f;

        public class Result
        {
            public Transform root;
            public Light pendantLight;
            public Light deskLamp;
            public Light windowFill;
            public Light crtGlow;
            public CRTMonitor crt;
            public MixerView mixer;
            public LeverView lever;
            public StampView stamps;
            public ClockView clock;
            public Transform printerSlot;
            public Transform standPose;
            public Transform tubeMouth;
            public Collider glass;
            public Material paperMat;
            public Transform slipRoot;
            public Transform trayPoint;
            public PrinterAnimator printer;
            public TubeView tube;
            public Transform printerRoot;
            public List<Renderer> lampHeads = new List<Renderer>();
        }

        private static Transform G(string name, Transform parent, Vector3 protoPos, float rotY = 0f)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = P(protoPos.x, protoPos.y, protoPos.z);
            g.transform.localRotation = Quaternion.Euler(0f, -rotY * Mathf.Rad2Deg, 0f);
            return g.transform;
        }

        private static GameObject B(string name, Transform parent, Vector3 size, Vector3 pos, Material m, bool shadow = true)
        {
            return Box(name, size, m, P(pos.x, pos.y, pos.z), parent, shadow);
        }

        private static GameObject Hit(GameObject host, Vector3 size, Vector3 center, ClickAction action, string tip, ViewId view = ViewId.Home, PostCategory cat = PostCategory.Sens)
        {
            var col = host.AddComponent<BoxCollider>();
            col.size = size;
            col.center = center;
            var rc = host.AddComponent<RoomClickable>();
            rc.action = action;
            rc.tooltip = tip;
            rc.view = view;
            rc.category = cat;
            return host;
        }

        private static GameObject FlatText(string name, string text, Transform parent, Vector3 localPos, float charSize, string colHex,
                                           TextAnchor anchor = TextAnchor.MiddleCenter, float yaw = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            // lying on a horizontal surface, readable from the player seat (+Z side looking towards -Z)
            go.transform.localRotation = Quaternion.Euler(90f, 180f + yaw, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.font = DefaultFont();
            go.GetComponent<MeshRenderer>().sharedMaterial = TextMaterial();
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = charSize;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = Hex(colHex);
            return go;
        }

        public static Result Build(ContentSet content, TuningData tuning, Transform parent)
        {
            var res = new Result();
            var root = new GameObject("Room");
            root.transform.SetParent(parent, false);
            res.root = root.transform;
            var t = root.transform;
            float dy = DeskY;

            var wallMat = TextureGen.TexMat("tex_wallpaper", new Vector2(6, 4), 0.9f);
            var wainMat = TextureGen.TexMat("tex_wainscot", new Vector2(5, 1), 0.7f);
            var floorMat = TextureGen.TexMat("tex_floor", new Vector2(3, 3), 0.6f);
            var ceilMat = TextureGen.TexMat("tex_ceiling", new Vector2(4, 4), 0.9f);
            var deskMat = TextureGen.TexMat("tex_desk", Vector2.one, 0.55f);
            var deskDark = TextureGen.TexMat("tex_deskdark", Vector2.one, 0.7f);
            var trim = Mat("#E6D6B8", 0.6f);
            var pipe = Mat("#8E97A6", 0.35f, 0.7f);
            var plasticDark = Mat("#24262E", 0.5f);
            var beige = Mat("#E0D6BD", 0.55f);
            var beigeDark = Mat("#BFB399", 0.6f);

            // structure
            B("Floor", t, new Vector3(5f, 0.05f, 4.6f), new Vector3(0f, -0.025f, 0.4f), floorMat, false);
            B("Ceiling", t, new Vector3(5f, 0.06f, 4.6f), new Vector3(0f, 3.03f, 0.4f), ceilMat, false);
            for (int i = 0; i < 4; i++) B("Beam", t, new Vector3(0.14f, 0.16f, 4.6f), new Vector3(-1.5f + i, 2.93f, 0.4f), deskDark);
            B("WainBack", t, new Vector3(5.7f, 0.75f, 0.12f), new Vector3(0f, 0.375f, -1.6f), wainMat);
            B("WallBackL", t, new Vector3(1.6f, 2.25f, 0.12f), new Vector3(-2.05f, 1.875f, -1.6f), wallMat);
            B("WallBackR", t, new Vector3(1.6f, 2.25f, 0.12f), new Vector3(2.05f, 1.875f, -1.6f), wallMat);
            B("WallBackTop", t, new Vector3(2.5f, 0.7f, 0.12f), new Vector3(0f, 2.65f, -1.6f), wallMat);
            B("TrimBack1", t, new Vector3(5.7f, 0.05f, 0.03f), new Vector3(0f, 0.77f, -1.53f), trim, false);
            B("TrimBack2", t, new Vector3(5.7f, 0.09f, 0.03f), new Vector3(0f, 0.045f, -1.53f), trim, false);
            foreach (float sx in new[] { -1.85f, 1.85f })
            {
                float s = Mathf.Sign(sx);
                B("WallSide", t, new Vector3(0.12f, 2.25f, 4.4f), new Vector3(sx, 1.875f, 0.6f), wallMat);
                B("WainSide", t, new Vector3(0.12f, 0.75f, 4.4f), new Vector3(sx, 0.375f, 0.6f), wainMat);
                B("TrimSide1", t, new Vector3(0.03f, 0.05f, 4.4f), new Vector3(sx - s * 0.07f, 0.77f, 0.6f), trim, false);
                B("TrimSide2", t, new Vector3(0.03f, 0.09f, 4.4f), new Vector3(sx - s * 0.07f, 0.045f, 0.6f), trim, false);
                B("TrimSide3", t, new Vector3(0.03f, 0.07f, 4.4f), new Vector3(sx - s * 0.07f, 2.88f, 0.6f), trim, false);
            }

            // window
            var frameMat = Mat("#9C4B32", 0.55f);
            B("Sill", t, new Vector3(2.66f, 0.07f, 0.36f), new Vector3(0f, 0.72f, -1.48f), trim);
            B("FrameTop", t, new Vector3(2.62f, 0.08f, 0.2f), new Vector3(0f, 2.31f, -1.58f), frameMat);
            B("FrameL", t, new Vector3(0.09f, 1.6f, 0.22f), new Vector3(-1.29f, 1.52f, -1.58f), frameMat);
            B("FrameR", t, new Vector3(0.09f, 1.6f, 0.22f), new Vector3(1.29f, 1.52f, -1.58f), frameMat);
            B("MullionV", t, new Vector3(0.035f, 1.56f, 0.05f), new Vector3(0f, 1.52f, -1.6f), frameMat);
            B("MullionH", t, new Vector3(2.5f, 0.03f, 0.05f), new Vector3(0f, 1.6f, -1.6f), frameMat);
            var glass = Quad("Glass", new Vector2(2.5f, 1.55f), Mat("#CFE3FF", 0.03f, 0f, null, 1f, 0.07f), P(0f, 1.52f, -1.62f), t);
            var glassCol = glass.AddComponent<BoxCollider>();
            glassCol.size = new Vector3(1f, 1f, 0.05f);
            var glassRc = glass.AddComponent<RoomClickable>();
            glassRc.action = ClickAction.Zoom;
            glassRc.view = ViewId.Window;
            glassRc.tooltip = "Janela: ver a rua";
            res.glass = glassCol;
            B("Blind", t, new Vector3(2.56f, 0.18f, 0.02f), new Vector3(0f, 2.2f, -1.47f), Mat("#E9DCC0", 0.95f));
            var rod = Cyl("BlindRod", 0.035f, 0.035f, 2.64f, Mat("#CDBE9E"), P(0f, 2.29f, -1.47f), t);
            rod.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Cyl("BlindString", 0.006f, 0.006f, 0.25f, Mat("#7A6A55"), P(0.9f, 1.83f, -1.46f), t);
            Sphere("BlindPull", 0.014f, Mat("#7A6A55"), P(0.9f, 1.7f, -1.46f), t, false);

            // pendant lamp
            var pend = G("Pendant", t, new Vector3(0.15f, 3f, 0.25f));
            Cyl("Cord", 0.004f, 0.004f, 0.62f, Mat("#222222"), P(0f, -0.31f, 0f), pend);
            Cyl("Shade", 0.1f, 0.1f, 0.14f, Mat("#2E4A3F", 0.5f, 0.3f), P(0f, -0.66f, 0f), pend);
            Sphere("Bulb", 0.045f, Unlit("#FFE2B0"), P(0f, -0.71f, 0f), pend);
            var pl = new GameObject("PendantLight").AddComponent<Light>();
            pl.transform.SetParent(t, false);
            pl.transform.localPosition = P(0.15f, 2.25f, 0.25f);
            pl.type = LightType.Point;
            pl.color = Hex("#FFC98A");
            pl.range = 5f;
            pl.intensity = 0.55f;
            pl.shadows = LightShadows.None;
            res.pendantLight = pl;

            // pipes along the left wall
            var p1 = Cyl("Pipe1", 0.03f, 0.03f, 4.4f, pipe, P(-1.72f, 2.72f, 0.6f), t);
            p1.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var p2 = Cyl("Pipe2", 0.02f, 0.02f, 4.4f, Mat("#B86B3E", 0.4f, 0.6f), P(-1.72f, 2.62f, 0.6f), t);
            p2.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            foreach (float z in new[] { -1.2f, -0.2f, 0.8f }) B("PipeClamp", t, new Vector3(0.08f, 0.12f, 0.03f), new Vector3(-1.75f, 2.68f, z), pipe);
            B("FuseBox", t, new Vector3(0.04f, 0.36f, 0.28f), new Vector3(1.77f, 2.25f, 1.15f), Mat("#B9BDB4", 0.5f, 0.3f));
            var fuseLab = Label("FuseLabel", "230V", new Vector2(0.12f, 0.05f), "#F2C230", "#1E2333", P(1.745f, 2.33f, 1.15f), t, 0.6f);
            fuseLab.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // desk
            B("DeskTop", t, new Vector3(2.95f, 0.07f, 1.45f), new Vector3(0f, 0.765f, 0.05f), deskMat);
            B("DeskFront", t, new Vector3(2.85f, 0.12f, 0.05f), new Vector3(0f, 0.67f, 0.74f), deskDark);
            foreach (float x in new[] { -1.1f, 1.1f })
            {
                B("Pedestal", t, new Vector3(0.6f, 0.66f, 1.3f), new Vector3(x, 0.33f, 0.05f), deskDark);
                for (int i = 0; i < 3; i++)
                {
                    B("Drawer", t, new Vector3(0.54f, 0.18f, 0.02f), new Vector3(x, 0.13f + i * 0.2f, 0.71f), deskMat);
                    B("Handle", t, new Vector3(0.12f, 0.015f, 0.02f), new Vector3(x, 0.16f + i * 0.2f, 0.73f), Mat("#C9A86A", 0.3f, 0.8f));
                }
            }
            Box("DeskMat", new Vector3(1.0f, 0.006f, 0.62f), TextureGen.TexMat("tex_mat", Vector2.one, 0.85f), P(0f, dy + 0.003f, 0.3f), t, false);

            // CRT
            var crt = G("CRT", t, new Vector3(-0.92f, dy, -0.22f), 0.32f);
            Cyl("CrtBase", 0.15f, 0.19f, 0.035f, beigeDark, P(0f, 0.018f, 0f), crt);
            B("CrtNeck", crt, new Vector3(0.16f, 0.06f, 0.14f), new Vector3(0f, 0.06f, 0f), beigeDark);
            B("CrtFront", crt, new Vector3(0.62f, 0.5f, 0.1f), new Vector3(0f, 0.35f, 0.2f), beige);
            B("CrtBackA", crt, new Vector3(0.52f, 0.42f, 0.16f), new Vector3(0f, 0.34f, 0.07f), beige);
            B("CrtBackB", crt, new Vector3(0.34f, 0.28f, 0.2f), new Vector3(0f, 0.34f, -0.1f), beige);
            for (int i = 0; i < 6; i++) B("Vent", crt, new Vector3(0.22f, 0.004f, 0.012f), new Vector3(0f, 0.55f, -0.1f + i * 0.035f), plasticDark, false);
            B("CrtBezel", crt, new Vector3(0.54f, 0.42f, 0.02f), new Vector3(0f, 0.36f, 0.245f), plasticDark, false);
            var screen = Quad("CrtScreen", new Vector2(0.5f, 0.375f), Unlit("#0F1A2E"), P(0f, 0.36f, 0.2565f), crt);
            Hit(screen, new Vector3(1f, 1f, 0.2f), Vector3.zero, ClickAction.Zoom, "Monitor do bairro (clica para ver de perto)", ViewId.Crt);
            Quad("CrtGlass", new Vector2(0.5f, 0.375f), Mat("#FFFFFF", 0.05f, 0f, null, 1f, 0.07f), P(0f, 0.36f, 0.259f), crt);
            Box("CrtLed", new Vector3(0.014f, 0.014f, 0.006f), Mat("#3DFF8A", 0.5f, 0f, "#3DFF8A", 2f), P(0.24f, 0.125f, 0.252f), crt, false);
            foreach (float x in new[] { 0.15f, 0.19f })
            {
                var k = Cyl("CrtKnob", 0.012f, 0.012f, 0.012f, plasticDark, P(x, 0.125f, 0.25f), crt, false);
                k.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Label("CrtBrand", "VZ-14 COLOR", new Vector2(0.11f, 0.02f), "#E0D6BD", "#6B6455", P(-0.18f, 0.125f, 0.251f), crt, 0.7f);
            // keyboard
            var kb = B("KeyboardBase", crt, new Vector3(0.5f, 0.028f, 0.18f), new Vector3(0f, 0.014f, 0.37f), beige);
            kb.transform.localRotation = Quaternion.Euler(0.05f * Mathf.Rad2Deg, 0f, 0f);
            for (int r = 0; r < 4; r++) B("Keys", crt, new Vector3(0.46f, 0.012f, 0.024f), new Vector3(0f, 0.036f + r * 0.0015f, 0.3f + r * 0.03f), Mat("#ECE5D3", 0.5f), false);
            B("Spacebar", crt, new Vector3(0.2f, 0.012f, 0.024f), new Vector3(0f, 0.034f, 0.435f), Mat("#ECE5D3", 0.5f), false);
            // clock
            B("ClockBody", crt, new Vector3(0.28f, 0.12f, 0.08f), new Vector3(0f, 0.66f, 0.19f), Mat("#2B2E38", 0.4f, 0.3f));
            var clockFace = Quad("ClockFace", new Vector2(0.26f, 0.104f), Unlit("#0B0D12"), P(0f, 0.66f, 0.2305f), crt);
            var clockMain = MakeText("ClockMain", "LIVRE", clockFace.transform, new Vector3(0f, 0.12f, -0.01f), 0.008f, "#FF6B4A");
            clockMain.transform.localScale = new Vector3(1f / 0.26f, 1f / 0.104f, 1f);
            var clockSub = MakeText("ClockSub", "TEMPO PARA DECIDIR", clockFace.transform, new Vector3(0f, -0.3f, -0.01f), 0.0036f, "#8C93AB");
            clockSub.transform.localScale = new Vector3(1f / 0.26f, 1f / 0.104f, 1f);
            var cv = crt.gameObject.AddComponent<ClockView>();
            cv.main = clockMain.GetComponent<TextMesh>();
            cv.sub = clockSub.GetComponent<TextMesh>();
            res.clock = cv;
            var crtMon = crt.gameObject.AddComponent<CRTMonitor>();
            crtMon.screen = screen.GetComponent<Renderer>();
            crtMon.circle = TextureGen.Spr("spr_disc");
            crtMon.ring = TextureGen.Spr("spr_ring");
            crtMon.diamond = TextureGen.Spr("spr_diamond");
            crtMon.triangle = TextureGen.Spr("spr_triangle");
            crtMon.star = TextureGen.Spr("spr_star");
            crtMon.house = TextureGen.Spr("spr_house");
            crtMon.scanlines = TextureGen.Spr("spr_scanlines");
            crtMon.vignette = TextureGen.Spr("spr_vignette");
            res.crt = crtMon;
            var glow = new GameObject("CrtGlow").AddComponent<Light>();
            glow.transform.SetParent(t, false);
            glow.transform.localPosition = P(-0.8f, 1.1f, 0f);
            glow.type = LightType.Point;
            glow.color = Hex("#5FD0FF");
            glow.range = 2f;
            glow.intensity = 0.5f;
            glow.shadows = LightShadows.None;
            res.crtGlow = glow;

            // printer
            var printer = G("Printer", t, new Vector3(0.95f, dy, -0.3f), -0.28f);
            res.printerRoot = printer;
            var pbody = B("PrinterBody", printer, new Vector3(0.62f, 0.12f, 0.36f), new Vector3(0f, 0.06f, 0f), Mat("#E4DED2", 0.5f));
            Hit(pbody, Vector3.one, Vector3.zero, ClickAction.None, "Impressora: os posts chegam aqui");
            B("PrinterTop", printer, new Vector3(0.62f, 0.06f, 0.13f), new Vector3(0f, 0.13f, -0.115f), Mat("#D6CFC0", 0.5f));
            var platen = Cyl("Platen", 0.022f, 0.022f, 0.52f, plasticDark, P(0f, 0.125f, -0.035f), printer, false);
            platen.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var knob = Cyl("PlatenKnob", 0.035f, 0.035f, 0.025f, plasticDark, P(0.325f, 0.11f, -0.035f), printer, false);
            knob.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var carriage = B("Carriage", printer, new Vector3(0.07f, 0.035f, 0.05f), new Vector3(0f, 0.112f, 0f), Mat("#4A4F5C", 0.4f, 0.4f));
            B("PrintRail", printer, new Vector3(0.52f, 0.006f, 0.006f), new Vector3(0f, 0.105f, 0.03f), pipe, false);
            B("PrinterSlot", printer, new Vector3(0.54f, 0.006f, 0.02f), new Vector3(0f, 0.161f, -0.07f), Mat("#111111", 0.9f), false);
            B("TearBar", printer, new Vector3(0.56f, 0.004f, 0.012f), new Vector3(0f, 0.166f, -0.055f), Mat("#B8BEC9", 0.3f, 0.8f), false);
            B("PrinterLed", printer, new Vector3(0.016f, 0.01f, 0.008f), new Vector3(0.22f, 0.075f, 0.181f), Mat("#3DFF8A", 0.5f, 0f, "#3DFF8A", 2f), false);
            B("PrinterLed2", printer, new Vector3(0.016f, 0.01f, 0.008f), new Vector3(0.25f, 0.075f, 0.181f), Mat("#FFB547", 0.5f, 0f, "#FFB547", 2f), false);
            Label("PrinterBrand", "VZ-MATRIX 9", new Vector2(0.16f, 0.022f), "#E4DED2", "#8A8270", P(-0.18f, 0.075f, 0.181f), printer, 0.7f);
            var fan = G("Fanfold", printer, new Vector3(0f, 0f, -0.34f));
            for (int i = 0; i < 10; i++) B("Fold", fan, new Vector3(0.5f, 0.006f, 0.2f), new Vector3(0f, 0.004f + i * 0.006f, 0f), Mat(i % 2 == 0 ? "#EAE4D6" : "#F4F0E6", 0.95f));
            // slot and slip rails
            var slot = new GameObject("SlipSlot");
            slot.transform.SetParent(printer, false);
            slot.transform.localPosition = P(0f, 0.162f, -0.07f);
            res.printerSlot = slot.transform;
            var pc = printer.gameObject.AddComponent<PrinterAnimator>();
            pc.carriage = carriage.transform;
            pc.platenKnob = knob.transform;
            res.printerRoot = printer;
            res.printer = pc;

            // reading stand
            var stand = G("Stand", t, new Vector3(0f, dy, 0.32f));
            var board = B("StandBoard", stand, new Vector3(0.62f, 0.02f, 0.46f), new Vector3(0f, 0.21f, 0f), deskDark);
            board.transform.localRotation = Quaternion.Euler(1.0f * Mathf.Rad2Deg, 0f, 0f);
            B("StandFoot", stand, new Vector3(0.64f, 0.045f, 0.04f), new Vector3(0f, 0.026f, 0.142f), deskDark);
            var clip = B("StandClip", stand, new Vector3(0.18f, 0.02f, 0.035f), new Vector3(0f, 0.41f, -0.11f), Mat("#B8BEC9", 0.25f, 0.85f));
            clip.transform.localRotation = Quaternion.Euler(1.0f * Mathf.Rad2Deg, 0f, 0f);
            foreach (float x in new[] { -0.22f, 0.22f }) B("StandPost", stand, new Vector3(0.03f, 0.24f, 0.03f), new Vector3(x, 0.12f, -0.04f), deskDark);
            var sp = new GameObject("StandPose");
            sp.transform.SetParent(t, false);
            sp.transform.localPosition = P(0f, dy + 0.1864f, 0.3481f);
            sp.transform.localRotation = Quaternion.Euler(1.0f * Mathf.Rad2Deg, 0f, 0f);
            res.standPose = sp.transform;
            // click target over the slip on the stand
            var standHit = new GameObject("StandHit");
            standHit.transform.SetParent(sp.transform, false);
            Hit(standHit, new Vector3(0.62f, 0.05f, 0.46f), Vector3.zero, ClickAction.Slip, "Post: clica para ler de perto");

            // stamps
            var stamps = new GameObject("Stamps");
            stamps.transform.SetParent(t, false);
            var sv = stamps.AddComponent<StampView>();
            sv.amplify = MakeStamp(stamps.transform, "StampAmp", "#2FB37E", "AMPLIFICAR", "A", new Vector3(0.62f, dy, 0.36f), ClickAction.Amplify);
            sv.hide = MakeStamp(stamps.transform, "StampHide", "#D6455F", "OCULTAR", "O", new Vector3(0.86f, dy, 0.48f), ClickAction.Hide);
            res.stamps = sv;

            // normal tray (basket)
            var tray = G("Tray", t, new Vector3(1.12f, dy, 0.06f), -0.2f);
            var wire = Mat("#3E4352", 0.35f, 0.7f);
            B("TrayBase", tray, new Vector3(0.32f, 0.006f, 0.25f), new Vector3(0f, 0.003f, 0f), wire);
            B("TrayFront", tray, new Vector3(0.32f, 0.07f, 0.006f), new Vector3(0f, 0.035f, 0.125f), wire);
            B("TrayBack", tray, new Vector3(0.32f, 0.07f, 0.006f), new Vector3(0f, 0.035f, -0.125f), wire);
            B("TrayL", tray, new Vector3(0.006f, 0.07f, 0.25f), new Vector3(-0.16f, 0.035f, 0f), wire);
            B("TrayR", tray, new Vector3(0.006f, 0.07f, 0.25f), new Vector3(0.16f, 0.035f, 0f), wire);
            Label("TrayLabel", "NORMAL · N", new Vector2(0.15f, 0.035f), "#F6F1E4", "#1E2333", P(0f, 0.05f, 0.1295f), tray, 0.6f);
            var trayHit = new GameObject("TrayHit");
            trayHit.transform.SetParent(tray, false);
            trayHit.transform.localPosition = P(0f, 0.04f, 0f);
            Hit(trayHit, new Vector3(0.34f, 0.1f, 0.27f), Vector3.zero, ClickAction.Normal, "Cesto NORMAL: publicar sem mexer (N)");
            var trayPoint = new GameObject("TrayPoint");
            trayPoint.transform.SetParent(tray, false);
            trayPoint.transform.localPosition = P(0f, 0.03f, 0f);
            res.trayPoint = trayPoint.transform;

            // lever
            var lever = G("Lever", t, new Vector3(1.3f, dy, 0.48f));
            var leverBase = B("LeverBase", lever, new Vector3(0.15f, 0.06f, 0.22f), new Vector3(0f, 0.03f, 0f), Mat("#2B2E38", 0.4f, 0.5f));
            B("LeverSlot", lever, new Vector3(0.025f, 0.004f, 0.14f), new Vector3(0f, 0.061f, 0f), Mat("#0B0C10"), false);
            var arm = G("LeverArm", lever, new Vector3(0f, 0.06f, 0f));
            arm.localRotation = Quaternion.Euler(-0.5f * Mathf.Rad2Deg, 0f, 0f);
            Cyl("LeverRod", 0.011f, 0.013f, 0.3f, Mat("#C9CED8", 0.2f, 0.9f), P(0f, 0.15f, 0f), arm);
            Sphere("LeverKnob", 0.038f, Mat("#E0473E", 0.25f), P(0f, 0.31f, 0f), arm, true);
            Label("LeverLabel", "PUBLICAR TURNO · ESPAÇO", new Vector2(0.14f, 0.03f), "#2B2E38", "#FFB547", P(0f, 0.03f, 0.111f), lever, 0.5f);
            var leverHit = new GameObject("LeverHit");
            leverHit.transform.SetParent(lever, false);
            leverHit.transform.localPosition = P(0f, 0.17f, 0f);
            Hit(leverHit, new Vector3(0.16f, 0.4f, 0.24f), Vector3.zero, ClickAction.Lever, "Alavanca: publicar o turno (Espaço)");
            var lv = lever.gameObject.AddComponent<LeverView>();
            lv.arm = arm;
            res.lever = lv;

            // mixer
            var mixer = G("Mixer", t, new Vector3(-0.74f, dy, 0.56f), 0.12f);
            var chassis = B("MixerChassis", mixer, new Vector3(0.64f, 0.05f, 0.36f), new Vector3(0f, 0.025f, 0f), Mat("#2B2E38", 0.45f, 0.3f));
            Hit(chassis, Vector3.one, Vector3.zero, ClickAction.Zoom, "Mesa de mistura: pesos do algoritmo (arrasta os faders)", ViewId.Mixer);
            var mv = mixer.gameObject.AddComponent<MixerView>();
            mv.surfaceWorldY = dy + 0.065f;
            string[] catNames = { "SENSACIONAL.", "COMUNIDADE", "ENTRETEN.", "PUBLICIDADE", "NOTÍCIAS" };
            for (int i = 0; i < 5; i++)
            {
                float x = -0.24f + i * 0.12f;
                var cat = (PostCategory)i;
                B("FaderSlot", mixer, new Vector3(0.014f, 0.004f, 0.24f), new Vector3(x, 0.0515f, 0f), Mat("#0D0E12"), false);
                var knobG = G("FaderKnob" + i, mixer, new Vector3(x, 0.065f, 0f));
                B("Cap", knobG, new Vector3(0.05f, 0.024f, 0.03f), Vector3.zero, Mat("#E8E4DA", 0.4f));
                string col = "#" + ColorUtility.ToHtmlStringRGB(ReactionStyle.CategoryColor(cat));
                B("Stripe", knobG, new Vector3(0.046f, 0.004f, 0.006f), new Vector3(0f, 0.013f, 0f), Mat(col, 0.5f, 0f, col, 1.5f), false);
                Hit(knobG.gameObject, new Vector3(0.07f, 0.05f, 0.05f), Vector3.zero, ClickAction.Fader, ReactionStyle.CategoryLabel(cat) + ": arrasta", ViewId.Home, cat);
                mv.knobs[i] = knobG;
                FlatText("CatLabel", catNames[i], mixer, P(x, 0.0525f, -0.158f), 0.0016f, col);
                var val = FlatText("Value", "50%", mixer, P(x, 0.0525f, 0.15f), 0.0021f, "#F1ECE0");
                mv.valueLabels[i] = val.GetComponent<TextMesh>();
            }
            res.mixer = mv;

            // desk clutter
            Cyl("PenCup", 0.045f, 0.04f, 0.11f, Mat("#E8B53A", 0.5f), P(0.3f, dy + 0.055f, -0.15f), t);
            string[] penCols = { "#C0392B", "#2980B9", "#27AE60", "#8E44AD", "#1E2333" };
            for (int i = 0; i < 5; i++)
            {
                var pen = Cyl("Pen", 0.005f, 0.005f, 0.19f, Mat(penCols[i]), P(0.3f + (i - 2) * 0.01f, dy + 0.14f, -0.15f + (i % 2 - 0.5f) * 0.02f), t, false);
                pen.transform.localRotation = Quaternion.Euler((i - 2) * 6f, 0f, (i - 2) * 5f);
            }
            // pneumatic tube
            var tubeGlass = Mat("#DDE8F0", 0.05f, 0f, null, 1f, 0.28f);
            Cyl("Tube", 0.065f, 0.065f, 1.4f, tubeGlass, P(1.58f, 1.68f, -0.42f), t, false);
            foreach (float y in new[] { 1.06f, 1.5f, 1.95f, 2.36f }) Cyl("TubeRing", 0.075f, 0.075f, 0.04f, pipe, P(1.58f, y, -0.42f), t);
            foreach (float y in new[] { 1.5f, 2.2f }) B("TubeBracket", t, new Vector3(0.16f, 0.035f, 0.035f), new Vector3(1.71f, y, -0.42f), pipe);
            var tubeRoot = new GameObject("TubeView");
            tubeRoot.transform.SetParent(t, false);
            var capsule = Cyl("Capsule", 0.05f, 0.05f, 0.14f, Mat("#C9A86A", 0.3f, 0.8f), P(1.58f, 1.1f, -0.42f), tubeRoot.transform);
            capsule.SetActive(false);
            var tv = tubeRoot.AddComponent<TubeView>();
            tv.capsule = capsule.transform;
            res.tube = tv;
            var mouth = Cyl("TubeMouth", 0.1f, 0.075f, 0.14f, pipe, P(1.58f, 0.98f, -0.42f), t);
            res.tubeMouth = mouth.transform;
            Cyl("TubeTop", 0.065f, 0.065f, 0.4f, pipe, P(1.58f, 2.55f, -0.42f), t);
            var tubeElbow = Cyl("TubeElbow", 0.065f, 0.065f, 0.3f, pipe, P(1.7f, 2.75f, -0.42f), t);
            tubeElbow.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // radio on the cabinet (position fixed after models are placed)
            var radioG = G("Radio", t, new Vector3(1.6f, 1.0f, -0.95f), -Mathf.PI / 2f + 0.25f);
            var rb = B("RadioBody", radioG, new Vector3(0.36f, 0.2f, 0.12f), new Vector3(0f, 0.1f, 0f), Mat("#C9C3B6", 0.45f));
            Hit(rb, Vector3.one, Vector3.zero, ClickAction.Radio, "Rádio Bairro FM");
            foreach (float x in new[] { -0.11f, 0.11f })
            {
                var spk = Cyl("Speaker", 0.06f, 0.06f, 0.01f, Mat("#3A3D47", 0.8f), P(x, 0.1f, 0.061f), radioG);
                spk.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Label("RadioDial", "88 · 92 · 96 · 100 · 104", new Vector2(0.1f, 0.03f), "#F2D58A", "#1E2333", P(0f, 0.15f, 0.0605f), radioG, 0.42f);
            var ant = Cyl("Antenna", 0.004f, 0.004f, 0.32f, pipe, P(0.14f, 0.33f, -0.03f), radioG, false);
            ant.transform.localRotation = Quaternion.Euler(0f, 0f, 0.5f * Mathf.Rad2Deg);

            // KayKit furniture
            var cab = Inst("kk-cabinet_medium", t, new InstOptions { length = 0.84f, pos = new Vector3(1.57f, 0f, -1.12f), rotY = -Mathf.PI / 2f });
            float cabTop = 1.0f;
            if (cab != null)
            {
                cabTop = RendererTop(cab);
                radioG.localPosition = P(1.6f, cabTop, -0.95f);
            }
            float ShelfTop(float y, float z)
            {
                var w = Inst("kk-shelf_A_small", t, new InstOptions { length = 0.6f, pos = new Vector3(1.66f, y, z), rotY = -Mathf.PI / 2f });
                return w != null ? RendererTop(w) : y + 0.03f;
            }
            float t1 = ShelfTop(1.36f, -1.05f), t2 = ShelfTop(1.84f, -1.05f);
            Inst("kk-book_set", t, new InstOptions { height = 0.15f, pos = new Vector3(1.68f, t1, -0.98f), rotY = -Mathf.PI / 2f });
            Inst("kk-cactus_small_A", t, new InstOptions { height = 0.12f, pos = new Vector3(1.68f, t1, -1.22f) });
            Inst("kk-jar_A_small", t, new InstOptions { height = 0.1f, pos = new Vector3(1.68f, t2, -1.2f) });
            Inst("kk-book_single", t, new InstOptions { height = 0.14f, pos = new Vector3(1.68f, t2, -1.05f), rotY = -Mathf.PI / 2f + 0.3f });
            Inst("plant", t, new InstOptions { height = 0.24f, pos = new Vector3(1.68f, t2, -0.9f) });
            Inst("kk-cabinet_small_decorated", t, new InstOptions { length = 0.55f, pos = new Vector3(-1.55f, 0f, 0.75f), rotY = Mathf.PI / 2f });
            var deskLampModel = Inst("kk-lamp_table", t, new InstOptions { height = 0.24f, pos = new Vector3(-1.4f, dy, -0.58f) });
            var lampLight = new GameObject("DeskLampLight").AddComponent<Light>();
            lampLight.transform.SetParent(t, false);
            lampLight.transform.localPosition = P(-1.33f, dy + 0.18f, -0.48f);
            lampLight.type = LightType.Point;
            lampLight.color = Hex("#FFB36B");
            lampLight.range = 4.5f;
            lampLight.intensity = 1.1f;
            lampLight.shadows = LightShadows.Soft;
            res.deskLamp = lampLight;
            Inst("kk-cactus_medium_A", t, new InstOptions { height = 0.2f, pos = new Vector3(-1.05f, 0.755f, -1.45f) });
            Inst("kk-pictureframe_standing_A", t, new InstOptions { height = 0.16f, pos = new Vector3(1.05f, 0.755f, -1.45f), rotY = -0.3f });
            Inst("kk-papertowel", t, new InstOptions { height = 0.2f, pos = new Vector3(1.62f, cabTop, -1.38f) });
            Inst("kk-cactus_small_A", t, new InstOptions { height = 0.12f, pos = new Vector3(1.64f, cabTop, -0.78f) });
            Inst("kk-rug_rectangle_stripes_A", t, new InstOptions { length = 2.6f, pos = new Vector3(0f, 0.002f, 0.9f) });
            Inst("mug-1", t, new InstOptions { height = 0.09f, pos = new Vector3(-0.32f, dy, 0.05f) });
            Inst("soda-can", t, new InstOptions { height = 0.1f, pos = new Vector3(-0.2f, dy, -0.45f), rotY = 0.4f });
            Frame(t, "kk-pictureframe_medium", -1.79f, 1.95f, -1.12f, Mathf.PI / 2f, 0.62f, 0);
            Frame(t, "kk-pictureframe_medium", -1.79f, 2.0f, 0.75f, Mathf.PI / 2f, 0.56f, 1);
            Frame(t, "kk-pictureframe_medium", 1.79f, 1.7f, 0.5f, -Mathf.PI / 2f, 0.5f, 2);

            // window fill (soft key light from the window)
            return res;
        }

        private static void Frame(Transform parent, string model, float x, float y, float z, float ry, float h, int poster)
        {
            var w = Inst(model, parent, new InstOptions { height = h, pos = new Vector3(x, y, z), rotY = ry });
            if (w == null) return;
            var posterObj = Quad("Poster" + poster, new Vector2(0.34f, 0.42f), PosterMat(poster), Vector3.zero, parent);
            posterObj.transform.localPosition = P(x + Mathf.Sin(ry) * 0.06f, y + h / 2f, z + Mathf.Cos(ry) * 0.06f);
            posterObj.transform.localRotation = Quaternion.Euler(0f, -ry * Mathf.Rad2Deg + 180f, 0f);
        }

        private static Material PosterMat(int i)
        {
            string[] cols = { "#FFB547", "#1E2333", "#F1ECE0" };
            return Unlit(cols[i % 3]);
        }

        private static float RendererTop(GameObject go)
        {
            float top = float.MinValue;
            foreach (var r in go.GetComponentsInChildren<Renderer>()) top = Mathf.Max(top, r.bounds.max.y);
            return top;
        }

        private static GameObject MakeText(string name, string text, Transform parent, Vector3 localPos, float charSize, string colHex)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var tm = go.AddComponent<TextMesh>();
            tm.font = DefaultFont();
            go.GetComponent<MeshRenderer>().sharedMaterial = TextMaterial();
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Hex(colHex);
            return go;
        }

        private static Transform MakeStamp(Transform parent, string name, string colHex, string label, string key, Vector3 pos, ClickAction action)
        {
            var g = G(name, parent, pos);
            Mat("#8E97A6", 0.35f, 0.75f);
            B("Tin", g, new Vector3(0.18f, 0.022f, 0.14f), new Vector3(0f, 0.011f, 0f), Mat("#8E97A6", 0.35f, 0.75f));
            B("Pad", g, new Vector3(0.155f, 0.004f, 0.115f), new Vector3(0f, 0.023f, 0f), Mat(colHex, 1f), false);
            B("Foot", g, new Vector3(0.12f, 0.022f, 0.085f), new Vector3(0f, 0.036f, 0f), Mat("#2A2A2E", 0.9f));
            B("Mount", g, new Vector3(0.13f, 0.026f, 0.095f), new Vector3(0f, 0.06f, 0f), Mat(colHex, 0.5f));
            Cyl("Handle", 0.02f, 0.03f, 0.13f, TextureGen.TexMat("tex_desk", Vector2.one, 0.5f), P(0f, 0.138f, 0f), g);
            Sphere("HandleKnob", 0.04f, TextureGen.TexMat("tex_desk", Vector2.one, 0.5f), P(0f, 0.215f, 0f), g, true);
            FlatText("StampLabel", label, g, P(0f, 0.0645f, 0f), 0.0014f, "#FFFFFF");
            FlatText("StampCard", label + " · " + key, g, P(0f, 0.0035f, 0.1f), 0.0017f, "#1E2333");
            B("CardBg", g, new Vector3(0.17f, 0.002f, 0.03f), new Vector3(0f, 0.002f, 0.1f), Mat("#F6F1E4", 0.9f), false);
            var hit = new GameObject("Hit");
            hit.transform.SetParent(g, false);
            hit.transform.localPosition = P(0f, 0.12f, 0.03f);
            Hit(hit, new Vector3(0.2f, 0.26f, 0.2f), Vector3.zero, action, "Carimbo " + label + " (" + key + ")");
            return g;
        }
    }
}
