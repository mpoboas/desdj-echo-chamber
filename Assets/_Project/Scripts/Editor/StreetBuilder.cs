using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static EchoChamber.Editor.SceneBuilderUtil;

namespace EchoChamber.Editor
{
    /// <summary>Builds the street, square, traffic and neighbours (port of buildStreetModels / buildCrossings).</summary>
    public static class StreetBuilder
    {
        private static System.Random rng;
        private static float Rnd(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        public class Result
        {
            public Transform root;
            public Transform staticRoot;
            public StreetView view;
            public TrafficSystem traffic;
            public List<Light> streetLights = new List<Light>();
            public List<NeighbourAgent> agents = new List<NeighbourAgent>();
        }

        private const float RS = 2.0f;
        private const float BS = 1.3f;

        public static Result Build(ContentSet content, TuningData tuning)
        {
            rng = new System.Random(20260301);
            var res = new Result();
            var root = new GameObject("Street");
            res.root = root.transform;
            var stat = new GameObject("Static");
            stat.transform.SetParent(root.transform, false);
            res.staticRoot = stat.transform;
            var dyn = new GameObject("Dynamic");
            dyn.transform.SetParent(root.transform, false);

            float gy = WorldMap.GroundY;
            float zRoad = WorldMap.Z(tuning.streetY);
            float zFront = -14.3f;
            float zB = zFront - BS / 2f;

            Box("Ground", new Vector3(70f, 0.1f, 40f), Mat("#8FA67A"), P(0f, gy - 0.06f, -16f), stat.transform, false);

            // Road along X with alternating lamp posts
            var lampHeads = new List<Renderer>();
            for (int i = -8; i <= 8; i++)
                Inst(i % 2 != 0 ? "city-road-straight" : "city-road-straight-lightposts", stat.transform,
                    new InstOptions { scale = RS, pos = new Vector3(i * RS, gy, zRoad), rotY = Mathf.PI / 2f, center = false });
            for (int i = -9; i <= 9; i++)
                Inst("city-pavement", stat.transform,
                    new InstOptions { scale = 1f, scaleZ = 0.76f, pos = new Vector3(i, gy + 0.004f, zFront + 0.38f), center = false, shadow = false });

            // The six buildings of the game, in street order
            var plan = new (PlaceId id, string model, float x, float sx, string tint)[]
            {
                (PlaceId.Cafe, "city-building-small-a", -3.6f, 1f, null),
                (PlaceId.Mercearia, "city-building-small-d", -2.25f, 1f, null),
                (PlaceId.CasasN, "city-building-small-c", -0.9f, 1f, null),
                (PlaceId.Junta, "city-building-small-b", 0.75f, 1.3f, "#F4EFE6"),
                (PlaceId.Livraria, "city-building-small-d", 2.35f, 1f, "#A6DCC8"),
                (PlaceId.CasasS, "city-building-small-b", 3.7f, 1f, "#FFD9C4"),
            };
            foreach (var (id, model, x, sx, tint) in plan)
            {
                var g = new GameObject("Place_" + id);
                g.transform.SetParent(stat.transform, false);
                g.transform.localPosition = P(x, gy, zB);
                Inst(model, g.transform, new InstOptions { scale = BS, scaleX = sx, tint = tint });
                var pd = content.FindPlace(id);
                string nm = pd.displayName.ToUpperInvariant();
                Label("Sign", nm, new Vector2(0.3f + nm.Length * 0.07f, 0.2f), "#1E2333", "#FFE9C2", P(0f, 1.0f, BS / 2f + 0.06f), g.transform, 0.6f);

                if (id == PlaceId.Mercearia || id == PlaceId.Livraria)
                {
                    var sh = Box("Shutter", new Vector3(0.62f, 0.5f * 0.02f, 0.03f), Mat("#8C93AB", 0.4f, 0.4f), P(0f, 0.82f - 0.5f * 0.02f / 2f, BS / 2f + 0.05f), g.transform, false);
                    if (res.view == null) res.view = null;
                    if (id == PlaceId.Mercearia) merceariaShutter = sh.transform; else livrariaShutter = sh.transform;
                }
                if (id == PlaceId.Cafe)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Cyl("TableTop", 0.1f, 0.1f, 0.025f, Mat("#ECE6D8"), P(-0.4f + i * 0.4f, 0.28f, BS / 2f + 0.4f), g.transform);
                        Cyl("TableLeg", 0.012f, 0.012f, 0.27f, Mat("#333333"), P(-0.4f + i * 0.4f, 0.135f, BS / 2f + 0.4f), g.transform);
                    }
                    var awn = Box("Awning", new Vector3(BS * 0.92f, 0.03f, 0.42f), Mat("#C0392B"), P(0f, 0.62f, BS / 2f + 0.2f), g.transform);
                    awn.transform.localRotation = Quaternion.Euler(0.28f * Mathf.Rad2Deg, 0f, 0f);
                }
            }

            // Fill the rest of the block, plus a back row for depth
            string[] fills = { "city-building-small-a", "city-building-small-b", "city-building-small-c", "city-building-small-d", "city-building-garage" };
            string[] tints = { "#FFFFFF", "#FFE8D6", "#E3F0FF", "#F3E6FF", "#E6FFF0" };
            foreach (float x in new[] { -9.2f, -7.9f, -6.6f, -5.2f, 5.05f, 6.4f, 7.75f, 9.1f })
                Inst(fills[(int)Rnd(0, 5)], stat.transform, new InstOptions { scale = BS, pos = new Vector3(x, gy, zB), tint = tints[(int)Rnd(0, 5)] });
            for (float x = -10.1f; x <= 10.2f; x += 1.6f)
                Inst(fills[(int)Rnd(0, 4)], stat.transform, new InstOptions { scale = BS * Rnd(1.0f, 1.15f), pos = new Vector3(x, gy, zB - 1.7f), tint = tints[(int)Rnd(0, 5)] });
            string[] skyline = { "#8A6F5C", "#9C8B7A", "#6E7C8C", "#B49B85" };
            for (int i = 0; i < 14; i++)
            {
                float h = Rnd(3f, 6f);
                Box("Skyline", new Vector3(1.6f, h, 1.5f), Mat(skyline[i % 4]), P(-11.7f + i * 1.8f, gy + h / 2f, -20.5f - (i % 2) * 0.4f), stat.transform);
            }

            // The square: paving, fountain, trees, benches
            var pr = content.FindPlace(PlaceId.Praca);
            float pz = WorldMap.Z(pr.rect[1] + pr.rect[3] / 2f);
            var praca = new GameObject("Place_Praca");
            praca.transform.SetParent(stat.transform, false);
            praca.transform.localPosition = P(0f, gy, pz);
            for (int i = -3; i <= 3; i++)
                foreach (float dz in new[] { -0.75f, 0.75f })
                {
                    bool corner = Mathf.Abs(i) == 3;
                    string mdl = corner ? (dz < 0f ? "city-grass-trees-tall" : "city-grass-trees") : "city-pavement";
                    Inst(mdl, stat.transform, new InstOptions { scale = 1.5f, pos = new Vector3(i * 1.5f, gy + (corner ? 0f : 0.012f), pz + dz), center = false, shadow = corner });
                }
            Inst("city-pavement-fountain", stat.transform, new InstOptions { scale = 1.5f, pos = new Vector3(0f, gy + 0.024f, pz), center = false });
            foreach (var (bx, bz, ry) in new[] { (-2.2f, pz - 1.05f, 0f), (2.2f, pz - 1.05f, 0f), (-2.2f, pz + 1.1f, Mathf.PI), (2.2f, pz + 1.1f, Mathf.PI) })
                Inst("bench", stat.transform, new InstOptions { length = 0.62f, pos = new Vector3(bx, gy + 0.05f, bz), rotY = ry });
            foreach (float tx in new[] { -6.5f, -5.2f, 5.2f, 6.5f })
                Inst("low-poly-tree", stat.transform, new InstOptions { height = 1.5f, pos = new Vector3(tx, gy, pz + Rnd(-0.6f, 0.6f)), rotY = Rnd(0f, 6f) });
            for (int i = -6; i <= 6; i++)
                for (int j = 0; j < 3; j++)
                    Inst("city-grass", stat.transform, new InstOptions { scale = 1.5f, pos = new Vector3(i * 1.5f, gy, pz + 2.25f + j * 1.5f), center = false, shadow = false });
            for (int i = -6; i <= 6; i++)
                if (Mathf.Abs(i) >= 4)
                    Inst("city-grass", stat.transform, new InstOptions { scale = 1.5f, scaleZ = 2f, pos = new Vector3(i * 1.5f, gy - 0.002f, pz), center = false, shadow = false });
            foreach (var (tx, tz) in new[] { (-4.6f, -6.6f), (4.4f, -6.9f), (-2.8f, -5.6f), (3.0f, -5.4f) })
                Inst("low-poly-tree", stat.transform, new InstOptions { height = Rnd(1.2f, 1.6f), pos = new Vector3(tx, gy, tz), rotY = Rnd(0f, 6f) });

            var lights = new GameObject("SquareLights");
            lights.transform.SetParent(praca.transform, false);
            string[] bulbCols = { "#FFB547", "#FF4F8B", "#4C7DFF" };
            for (int i = 0; i < 16; i++)
                Sphere("Bulb", 0.045f, Unlit(bulbCols[i % 3]), P(-3f + i * 0.4f, 1.25f - Mathf.Sin(i / 15f * Mathf.PI) * 0.35f, -0.2f), lights.transform);
            lights.SetActive(false);

            // Crossings, signals
            BuildCrossings(tuning, dyn.transform, zRoad);

            // Night street lights
            foreach (float x in new[] { -4f, 0f, 4f })
            {
                var lg = new GameObject("StreetLight");
                lg.transform.SetParent(dyn.transform, false);
                lg.transform.localPosition = P(x, gy + 1.3f, zRoad + 0.9f);
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = Hex("#FFD27A");
                l.range = 5f;
                l.intensity = 0f;
                l.shadows = LightShadows.None;
                res.streetLights.Add(l);
            }

            // Police car
            var police = new GameObject("PoliceCar");
            police.transform.SetParent(dyn.transform, false);
            Inst("police-car", police.transform, new InstOptions { length = 0.95f, rotY = Mathf.PI / 2f });
            var blue = Box("LightBlue", new Vector3(0.07f, 0.04f, 0.09f), Unlit("#3366FF"), P(0f, 0.5f, -0.07f), police.transform, false);
            var red = Box("LightRed", new Vector3(0.07f, 0.04f, 0.09f), Unlit("#FF3355"), P(0f, 0.5f, 0.07f), police.transform, false);
            police.transform.position = new Vector3(0f, gy, zRoad + 0.42f);
            police.SetActive(false);

            // Ambient traffic
            var trafficGo = new GameObject("Traffic");
            trafficGo.transform.SetParent(dyn.transform, false);
            var ts = trafficGo.AddComponent<TrafficSystem>();
            string[] carModels = { "taxi", "sedan", "van", "hatchback", "delivery-truck", "sedan" };
            for (int i = 0; i < carModels.Length; i++)
            {
                int dir = i % 2 != 0 ? 1 : -1;
                float cx = -11f + i * 3.9f + Rnd(-0.3f, 0.3f);
                var car = Inst(carModels[i], trafficGo.transform, new InstOptions
                {
                    length = carModels[i] == "delivery-truck" ? 1.15f : 0.9f,
                    rotY = dir > 0 ? Mathf.PI / 2f : -Mathf.PI / 2f,
                    pos = new Vector3(cx, gy, zRoad + (dir > 0 ? 0.42f : -0.42f))
                });
                if (car != null)
                    ts.cars.Add(new TrafficSystem.Car { body = car.transform, dir = dir, speed = Rnd(1.3f, 2.0f) });
            }
            res.traffic = ts;

            // Street view sync
            var sv = dyn.AddComponent<StreetView>();
            sv.merceariaShutter = merceariaShutter;
            sv.livrariaShutter = livrariaShutter;
            sv.police = police.transform;
            sv.policeBlue = blue;
            sv.policeRed = red;
            sv.squareLights = lights;
            res.view = sv;

            BuildNavMesh(stat);
            BuildNeighbours(content, tuning, dyn.transform, res);
            return res;
        }

        private static Transform merceariaShutter;
        private static Transform livrariaShutter;

        private static void BuildCrossings(TuningData tuning, Transform parent, float zRoad)
        {
            float gy = WorldMap.GroundY;
            float roadTop = gy + 0.052f;
            var white = Mat("#F4F1E8", 0.7f);
            var pole = Mat("#2B2E38", 0.4f, 0.6f);
            foreach (float lx in new[] { tuning.crosswalkLeftX, tuning.crosswalkRightX })
            {
                float cx = WorldMap.ProtoX(lx);
                for (int i = 0; i < 6; i++)
                    Box("Zebra", new Vector3(0.62f, 0.004f, 0.17f), white, P(cx, roadTop, zRoad - 0.83f + i * 0.33f), parent, false);

                foreach (var (dir, sx, sz) in new[] { (1, cx - 0.6f, zRoad + 1.08f), (-1, cx + 0.6f, zRoad - 1.08f) })
                {
                    var g = new GameObject("CarSignal");
                    g.transform.SetParent(parent, false);
                    g.transform.localPosition = P(sx, gy + 0.1f, sz);
                    Cyl("Pole", 0.025f, 0.03f, 1.15f, pole, P(0f, 0.575f, 0f), g.transform);
                    var head = new GameObject("Head");
                    head.transform.SetParent(g.transform, false);
                    head.transform.localPosition = P(0f, 1.18f, 0f);
                    float ry = dir > 0 ? -Mathf.PI / 2f + 0.55f : Mathf.PI / 2f + 0.55f;
                    head.transform.localRotation = Quaternion.Euler(0f, -ry * Mathf.Rad2Deg, 0f);
                    Box("Body", new Vector3(0.13f, 0.34f, 0.1f), pole, Vector3.zero, head.transform);
                    var lamp = g.AddComponent<SignalLamp>();
                    lamp.kind = SignalLamp.Kind.Car;
                    var defs = new (string col, float y)[] { ("#ff3b3b", 0.105f), ("#ffb547", 0f), ("#3dff8a", -0.105f) };
                    for (int k = 0; k < 3; k++)
                    {
                        var q = Quad("Lamp", new Vector2(0.076f, 0.076f), Unlit(defs[k].col), P(0f, defs[k].y, 0.052f), head.transform);
                        // Quad faces +Z of the head; the signal looks toward -Z in the prototype head frame (z=.052 front).
                        if (k == 0) lamp.red = q.GetComponent<Renderer>();
                        if (k == 1) lamp.yellow = q.GetComponent<Renderer>();
                        if (k == 2) lamp.green = q.GetComponent<Renderer>();
                        Box("Visor", new Vector3(0.09f, 0.01f, 0.05f), pole, P(0f, defs[k].y + 0.045f, 0.07f), head.transform, false);
                    }
                }

                foreach (var (sz, ry) in new[] { (zRoad - 1.1f, 0f), (zRoad + 1.1f, Mathf.PI) })
                {
                    var g = new GameObject("PedSignal");
                    g.transform.SetParent(parent, false);
                    g.transform.localPosition = P(cx + 0.42f, gy + 0.1f, sz);
                    g.transform.localRotation = Quaternion.Euler(0f, -ry * Mathf.Rad2Deg, 0f);
                    Cyl("Pole", 0.02f, 0.025f, 0.8f, pole, P(0f, 0.4f, 0f), g.transform);
                    Box("Body", new Vector3(0.12f, 0.24f, 0.08f), pole, P(0f, 0.86f, 0f), g.transform);
                    var lamp = g.AddComponent<SignalLamp>();
                    lamp.kind = SignalLamp.Kind.Pedestrian;
                    lamp.pedWalk = Quad("Walk", new Vector2(0.085f, 0.085f), Unlit("#3dff8a"), P(0f, 0.805f, 0.042f), g.transform).GetComponent<Renderer>();
                    lamp.pedStop = Quad("Stop", new Vector2(0.085f, 0.085f), Unlit("#ff3b3b"), P(0f, 0.915f, 0.042f), g.transform).GetComponent<Renderer>();
                    Box("Button", new Vector3(0.05f, 0.07f, 0.04f), Mat("#E8C547"), P(0f, 0.55f, 0.03f), g.transform, false);
                }
            }
        }

        private static void BuildNavMesh(GameObject stat)
        {
            NavMeshSettingsEditor.EnsureSmallAgent();
            var surface = stat.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.defaultArea = 0;
            surface.overrideVoxelSize = true;
            surface.voxelSize = 0.02f;
            surface.BuildNavMesh();
            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes/NavMesh");
            UnityEditor.AssetDatabase.DeleteAsset("Assets/_Project/Scenes/NavMesh/NavMesh-Street.asset");
            UnityEditor.AssetDatabase.CreateAsset(surface.navMeshData, "Assets/_Project/Scenes/NavMesh/NavMesh-Street.asset");
        }

        private static readonly string[] NpcModels =
        {
            "survivor-female", "male", "skater-female", "skater-male", "cyborg-female", "survivor-male", "survivor-female",
            "male", "skater-female", "survivor-male", "cyborg-female", "skater-male", "survivor-female", "male"
        };

        private static void BuildNeighbours(ContentSet content, TuningData tuning, Transform parent, Result res)
        {
            var group = new GameObject("Neighbours");
            group.transform.SetParent(parent, false);
            var circle = CircleSprite();
            for (int i = 0; i < content.neighbours.Count; i++)
            {
                var nd = content.neighbours[i];
                var go = new GameObject("Neighbour_" + nd.name);
                go.transform.SetParent(group.transform, false);
                go.transform.position = WorldMap.ToWorld(500f, 300f);

                var agent = go.AddComponent<NavMeshAgent>();
                agent.radius = 0.07f;
                agent.height = 0.62f;
                agent.baseOffset = 0f;
                agent.angularSpeed = 540f;
                agent.autoBraking = false;
                agent.stoppingDistance = 0f;
                agent.avoidancePriority = 30 + i;
                agent.enabled = false;

                var visual = new GameObject("Visual");
                visual.transform.SetParent(go.transform, false);
                var body = Inst(NpcModels[i % NpcModels.Length], visual.transform, new InstOptions { height = 0.66f });
                if (body != null) NormaliseCharacter(body.transform);

                var ring = Cyl("Ring", 0.17f, 0.17f, 0.004f, Unlit(nd.color), new Vector3(0f, 0.012f, 0f), visual.transform, false);

                var sign = new GameObject("ProtestSign");
                sign.transform.SetParent(visual.transform, false);
                Box("Board", new Vector3(0.24f, 0.15f, 0.02f), Mat("#FFB547"), new Vector3(0f, 0.95f, 0f), sign.transform, false);
                Box("Stick", new Vector3(0.02f, 0.32f, 0.02f), Mat("#5A3E2B"), new Vector3(0f, 0.74f, 0f), sign.transform, false);
                sign.SetActive(false);

                var icon = new GameObject("ReactionIcon");
                icon.transform.SetParent(go.transform, false);
                icon.transform.localPosition = new Vector3(0f, 0.92f, 0f);
                icon.transform.localScale = Vector3.one * 0.3f;
                var sr = icon.AddComponent<SpriteRenderer>();
                sr.sprite = circle;
                sr.sortingOrder = 5;
                var glyph = new GameObject("Glyph");
                glyph.transform.SetParent(icon.transform, false);
                glyph.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                var tm = glyph.AddComponent<TextMesh>();
                tm.font = DefaultFont();
                tm.GetComponent<MeshRenderer>().sharedMaterial = TextMaterial();
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.fontSize = 64;
                tm.characterSize = 0.07f;
                tm.color = Color.white;
                tm.text = "";
                glyph.GetComponent<MeshRenderer>().sortingOrder = 6;
                icon.SetActive(false);

                var na = go.AddComponent<NeighbourAgent>();
                na.index = i;
                na.visual = visual.transform;
                na.iconDisc = sr;
                na.iconGlyph = tm;
                na.protestSign = sign;
                res.agents.Add(na);
            }
        }

        /// <summary>Scale the rigged character to 0.66 tall using its head bone, and drop the imported mesh offset.</summary>
        private static void NormaliseCharacter(Transform wrapper)
        {
            Transform head = null;
            float bottom = float.MaxValue;
            foreach (var t in wrapper.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Head") head = t;
                if (t.name.EndsWith("Toes_end") || t.name.EndsWith("Foot")) bottom = Mathf.Min(bottom, t.position.y);
            }
            if (head == null) return;
            float top = head.position.y + 0.25f * wrapper.lossyScale.y;
            float floor = Mathf.Min(bottom, wrapper.position.y);
            float h = top - floor;
            if (h > 0.01f) wrapper.localScale *= 0.66f / h;
        }
    }
}
