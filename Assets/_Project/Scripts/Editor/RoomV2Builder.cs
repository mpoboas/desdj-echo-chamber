using System.Collections.Generic;
using UnityEngine;
using static EchoChamber.Editor.SceneBuilderUtil;

namespace EchoChamber.Editor
{
    /// <summary>Builds the control room (basement) with the interactive desk objects, from the prototype layout.</summary>
    public static class RoomV2Builder
    {
        public const float DeskY = 0.8f;

        public class Result
        {
            public Transform root;
            public Light pendantLight;
            public Light deskLamp;
            public Light lcdGlow;
            public Renderer lcdScreen;
            public BoxCollider lcdCollider;
            public Transform lcdCenter;
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
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = charSize;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = Hex(colHex);
            return go;
        }

        public static Result Build(Material lcdMaterial, Transform parent)
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

            // blinds closed (day 1 and 2): horizontal slats over the whole window
            {
                var slatMat = Mat("#CDBF9E", 0.9f);
                var slatDark = Mat("#8C7F66", 0.9f);
                for (int i = 0; i < 26; i++)
                {
                    float y = 0.79f + i * 0.0595f;
                    B("Slat", t, new Vector3(2.5f, 0.05f, 0.012f), new Vector3(0f, y, -1.515f), i % 2 == 0 ? slatMat : slatDark, false);
                }
                B("BlindsHeadrail", t, new Vector3(2.56f, 0.05f, 0.04f), new Vector3(0f, 2.31f, -1.5f), slatDark);
            }
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


            // LCD 16:9 (replaces the CRT): the Central de Feeds is drawn into lcdMaterial
            {
                float sw = 0.8f, sh = 0.45f;
                float cy = dy + 0.11f + sh / 2f + 0.03f;
                var lcd = G("LCD", t, new Vector3(0f, dy, -0.2f));
                var black = Mat("#0B0B0F", 0.35f, 0.2f);
                B("LcdFoot", lcd, new Vector3(0.34f, 0.018f, 0.2f), new Vector3(0f, 0.009f, 0.03f), Mat("#1B1B22", 0.4f, 0.6f));
                B("LcdNeck", lcd, new Vector3(0.07f, 0.12f, 0.035f), new Vector3(0f, 0.07f, -0.03f), Mat("#1B1B22", 0.4f, 0.6f));
                B("LcdBack", lcd, new Vector3(sw + 0.06f, sh + 0.06f, 0.035f), new Vector3(0f, cy - dy, -0.03f), black);
                B("LcdBezel", lcd, new Vector3(sw + 0.06f, sh + 0.06f, 0.012f), new Vector3(0f, cy - dy, 0.0f), black);
                var screenQuad = Quad("LcdScreen", new Vector2(sw, sh), lcdMaterial, P(0f, cy - dy, 0.0075f), lcd);
                var rend = screenQuad.GetComponent<Renderer>();
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                foreach (var oc in screenQuad.GetComponents<Collider>()) Object.DestroyImmediate(oc);
                var mc = screenQuad.AddComponent<BoxCollider>();
                mc.size = new Vector3(1f, 1f, 0.002f);
                var rc = screenQuad.AddComponent<RoomClickable>();
                rc.action = ClickAction.Zoom;
                rc.view = ViewId.Crt;
                rc.tooltip = "Central de Feeds (clica para ver de perto)";
                Box("LcdLed", new Vector3(0.012f, 0.006f, 0.004f), Mat("#3DFF8A", 0.5f, 0f, "#3DFF8A", 2f), P(0.3f, cy - dy - sh / 2f - 0.017f, 0.008f), lcd, false);
                res.lcdScreen = rend;
                res.lcdCollider = mc;
                res.lcdCenter = screenQuad.transform;
                var glow = new GameObject("LcdGlow").AddComponent<Light>();
                glow.transform.SetParent(t, false);
                glow.transform.localPosition = P(0f, cy, 0.35f);
                glow.type = LightType.Point;
                glow.color = Hex("#7FA2FF");
                glow.range = 2.2f;
                glow.intensity = 0.45f;
                glow.shadows = LightShadows.None;
                res.lcdGlow = glow;
            }
            // phone
            var phone = G("Phone", t, new Vector3(0.4f, dy, -0.44f), -0.35f);
            var pbase = B("PhoneBase", phone, new Vector3(0.2f, 0.09f, 0.2f), new Vector3(0f, 0.045f, 0f), Mat("#B83227", 0.3f));
            Hit(pbase, Vector3.one, Vector3.zero, ClickAction.Phone, "Telefone");
            
            var dial = Cyl("Dial", 0.055f, 0.055f, 0.01f, Mat("#F1ECE0", 0.4f), P(0f, 0.095f, 0.03f), phone);
            dial.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            var handset = G("Handset", phone, new Vector3(0f, 0.115f, -0.005f));
            
            var hmat = Mat("#B83227", 0.3f);
            var hb = Cyl("HandsetBar", 0.017f, 0.017f, 0.24f, hmat, Vector3.zero, handset);
            hb.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            foreach (float x in new[] { -0.12f, 0.12f }) Cyl("HandsetCup", 0.03f, 0.024f, 0.035f, hmat, P(x, -0.005f, 0f), handset);
            Hit(handset.gameObject, new Vector3(0.3f, 0.08f, 0.08f), Vector3.zero, ClickAction.Phone, "Telefone");
            handset.gameObject.AddComponent<PhoneView>().handset = handset;

            // desk clutter
            Cyl("PenCup", 0.045f, 0.04f, 0.11f, Mat("#E8B53A", 0.5f), P(0.75f, dy + 0.055f, 0.1f), t);
            string[] penCols = { "#C0392B", "#2980B9", "#27AE60", "#8E44AD", "#1E2333" };
            for (int i = 0; i < 5; i++)
            {
                var pen = Cyl("Pen", 0.005f, 0.005f, 0.19f, Mat(penCols[i]), P(0.75f + (i - 2) * 0.01f, dy + 0.14f, 0.1f + (i % 2 - 0.5f) * 0.02f), t, false);
                pen.transform.localRotation = Quaternion.Euler((i - 2) * 6f, 0f, (i - 2) * 5f);
            }
            var nb = B("Notebook", t, new Vector3(0.2f, 0.016f, 0.26f), new Vector3(0.1f, dy + 0.008f, -0.45f), Mat("#2E4A3F", 0.7f));
            nb.transform.localRotation = Quaternion.Euler(0f, 0.3f * Mathf.Rad2Deg, 0f);
            var pg = B("NotebookPage", t, new Vector3(0.19f, 0.004f, 0.25f), new Vector3(0.1f, dy + 0.018f, -0.45f), Mat("#F4F0E6", 0.95f));
            pg.transform.localRotation = Quaternion.Euler(0f, 0.3f * Mathf.Rad2Deg, 0f);

            // KayKit furniture
            var cab = Inst("kk-cabinet_medium", t, new InstOptions { length = 0.84f, pos = new Vector3(1.57f, 0f, -1.12f), rotY = -Mathf.PI / 2f });
            float cabTop = 1.0f;
            if (cab != null)
            {
                cabTop = RendererTop(cab);
                
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
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = charSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Hex(colHex);
            return go;
        }

    }
}
