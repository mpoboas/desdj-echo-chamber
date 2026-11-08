using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static EchoChamber.Editor.SceneBuilderUtil;

namespace EchoChamber.Editor
{
    /// <summary>Builds Game.unity (v2): the control room with the LCD that shows the Central de Feeds.</summary>
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Game.unity";
        private const string Gen = "Assets/_Project/Art/Generated";
        private const string UiDir = "Assets/_Project/UI";

        [MenuItem("Echo Chamber/Build Game Scene (v2)")]
        public static void Build()
        {
            ResetCache();
            FeedContentImporter.ImportAll();
            FeedClipImporter.ImportAll();
            TextureGen.GenerateAll();
            Directory.CreateDirectory(Gen);
            Directory.CreateDirectory(UiDir);

            var rt = EnsureRenderTexture();
            var mat = EnsureLcdMaterial(rt);
            var panel = EnsurePanelSettings(rt);
            var hudPanel = EnsureHudPanelSettings();
            var hudSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UiDir + "/FeedHud.uss");
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UiDir + "/CentralFeeds.uss");
            var content = AssetDatabase.LoadAssetAtPath<FeedContentAsset>(FeedContentImporter.AssetPath);
            var clips = AssetDatabase.LoadAssetAtPath<FeedClipCatalog>(FeedClipImporter.AssetPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#1A1310");
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 40f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.31f, 0.33f);
            RenderSettings.fog = false;
            BuildPostProcessing();

            var room = RoomV2Builder.Build(mat, null);
            room.pendantLight.intensity = 0.9f;
            room.deskLamp.intensity = 1.3f;

            var rigGo = new GameObject("CameraRig");
            var rig = rigGo.AddComponent<CameraRig>();
            rig.mainCamera = cam;
            float cy = RoomV2Builder.DeskY + 0.11f + 0.225f + 0.03f;
            var viewDefs = new (ViewId id, Vector3 p, Vector3 l)[]
            {
                (ViewId.Home, new Vector3(0f, 1.9f, 2.25f), new Vector3(0f, 1.0f, -1.0f)),
                (ViewId.Crt, new Vector3(0f, cy, 0.3f), new Vector3(0f, cy, -0.2f)),
            };
            foreach (var vd in viewDefs)
            {
                var vgo = new GameObject("VCam_" + vd.id);
                vgo.transform.SetParent(rigGo.transform, false);
                var vcam = vgo.AddComponent<CinemachineCamera>();
                rig.views.Add(new CameraRig.View { id = vd.id, position = P(vd.p.x, vd.p.y, vd.p.z), lookAt = P(vd.l.x, vd.l.y, vd.l.z), vcam = vcam });
            }
            rig.Setup();
            foreach (var v in rig.views) v.vcam.Priority.Value = v.id == ViewId.Home ? 20 : 0;
            camGo.transform.position = rig.views[0].position;
            camGo.transform.rotation = Quaternion.LookRotation((rig.views[0].lookAt - rig.views[0].position).normalized);

            var inputGo = new GameObject("RoomInput");
            inputGo.AddComponent<RoomInput>().cam = cam;
            UiFactoryEditor.EnsureEventSystem();

            var gameGo = new GameObject("FeedGame");
            var doc = gameGo.AddComponent<UIDocument>();
            doc.panelSettings = panel;
            var ctl = gameGo.AddComponent<FeedGameController>();
            ctl.content = content;
            ctl.clips = clips;
            ctl.document = doc;
            ctl.sheet = sheet;
            ctl.panelSettings = panel;
            ctl.screenTexture = rt;
            ctl.cam = cam;
            ctl.screenCollider = room.lcdCollider;
            var hudGo = new GameObject("FeedHud");
            var hudDoc = hudGo.AddComponent<UIDocument>();
            hudDoc.panelSettings = hudPanel;
            ctl.hudDocument = hudDoc;
            ctl.hudSheet = hudSheet;

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[EchoChamber] Built " + ScenePath);
        }

        private static RenderTexture EnsureRenderTexture()
        {
            string path = Gen + "/LCD.renderTexture";
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt != null && (rt.depth < 24 || rt.width != 2560))
            {
                AssetDatabase.DeleteAsset(path);
                rt = null;
            }
            if (rt == null)
            {
                // depth/stencil is required: UI Toolkit masks (overflow hidden, rounded corners) use the stencil buffer
                rt = new RenderTexture(2560, 1440, 24, RenderTextureFormat.ARGB32) { name = "LCD", useMipMap = false, antiAliasing = 1, filterMode = FilterMode.Bilinear };
                AssetDatabase.CreateAsset(rt, path);
            }
            return rt;
        }

        private static Material EnsureLcdMaterial(RenderTexture rt)
        {
            string path = Gen + "/LCD_Mat.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetTexture("_BaseMap", rt);
            m.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static PanelSettings EnsurePanelSettings(RenderTexture rt)
        {
            string tss = UiDir + "/FeedTheme.tss";
            if (!File.Exists(tss))
            {
                File.WriteAllText(tss, "@import url(\"unity-theme://default\");\n");
                AssetDatabase.ImportAsset(tss);
            }
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(tss);
            string path = UiDir + "/FeedPanelSettings.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(ps, path);
            }
            ps.themeStyleSheet = theme;
            ps.targetTexture = rt;
            ps.scaleMode = PanelScaleMode.ConstantPixelSize;
            ps.scale = 1.6f; // 2560x1440 texture, 1600x900 layout
            ps.clearColor = true;
            ps.colorClearValue = new Color(0.04f, 0.05f, 0.08f, 1f);
            EditorUtility.SetDirty(ps);
            return ps;
        }

        private static PanelSettings EnsureHudPanelSettings()
        {
            string tss = UiDir + "/FeedTheme.tss";
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(tss);
            string path = UiDir + "/FeedHudPanelSettings.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps == null)
            {
                ps = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(ps, path);
            }
            ps.themeStyleSheet = theme;
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match = 0.5f;
            ps.sortingOrder = 10;
            EditorUtility.SetDirty(ps);
            return ps;
        }

        private static void BuildPostProcessing()
        {
            string path = Gen + "/GameVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            profile.components.Clear();
            profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.None);
            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.25f);
            vig.smoothness.Override(0.5f);
            EditorUtility.SetDirty(profile);
            var go = new GameObject("Global Volume");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;
        }
    }
}
