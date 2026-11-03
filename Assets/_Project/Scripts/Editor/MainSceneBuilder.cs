using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using static EchoChamber.Editor.SceneBuilderUtil;

namespace EchoChamber.Editor
{
    /// <summary>Builds Main.unity from scratch: lights, camera, simulation host, street (and later the room and UI).</summary>
    public static class MainSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Main.unity";

        [MenuItem("Echo Chamber/Build Main Scene")]
        public static void BuildMainScene()
        {
            ResetCache();
            var content = AssetDatabase.LoadAssetAtPath<GameContent>(ContentImporter.ContentAssetPath);
            var tuning = AssetDatabase.LoadAssetAtPath<TuningConfig>(ContentImporter.TuningAssetPath);
            if (content == null || tuning == null)
            {
                ContentImporter.ImportAll();
                content = AssetDatabase.LoadAssetAtPath<GameContent>(ContentImporter.ContentAssetPath);
                tuning = AssetDatabase.LoadAssetAtPath<TuningConfig>(ContentImporter.TuningAssetPath);
            }

            Directory.CreateDirectory("Assets/_Project/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Simulation host
            var host = new GameObject("GameManager");
            var gm = host.AddComponent<GameManager>();
            gm.content = content;
            gm.tuning = tuning;

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#BFD9EC");
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 80f;
            camGo.AddComponent<AudioListener>();
            var urpCam = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            // Lights
            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = Hex("#FFF1DC");
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.position = P(-4f, 9f, -14f);
            sunGo.transform.rotation = Quaternion.LookRotation(P(4f, -9f, 6f).normalized);

            var winGo = new GameObject("WindowLight");
            var win = winGo.AddComponent<Light>();
            win.type = LightType.Directional;
            win.color = Hex("#CFE3FF");
            win.intensity = 0.7f;
            win.shadows = LightShadows.Soft;
            winGo.transform.position = P(0f, 2.4f, -4f);
            winGo.transform.rotation = Quaternion.LookRotation(P(0f, -1.6f, 4.5f).normalized);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.56f, 0.64f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 14f;
            RenderSettings.fogEndDistance = 34f;
            RenderSettings.fogColor = Hex("#BFD9EC");
            RenderSettings.skybox = null;

            BuildPostProcessing();

            // Street
            TextureGen.GenerateAll();
            var street = StreetBuilder.Build(content.content, tuning.data);

            // Room
            var room = RoomBuilder.Build(content.content, tuning.data, null);
            var toggle = room.root.gameObject.AddComponent<GlassClickToggle>();
            toggle.glass = room.glass;

            // Camera rig (Cinemachine virtual cameras for each view)
            var rigGo = new GameObject("CameraRig");
            var rig = rigGo.AddComponent<CameraRig>();
            rig.mainCamera = cam;
            var viewDefs = new (ViewId id, Vector3 p, Vector3 l)[]
            {
                (ViewId.Home, new Vector3(0f, 1.9f, 2.25f), new Vector3(0f, 1.0f, -1.0f)),
                (ViewId.Crt, new Vector3(-0.6f, 1.2f, 0.62f), new Vector3(-0.93f, 1.12f, -0.2f)),
                (ViewId.Mixer, new Vector3(-0.68f, 1.42f, 0.95f), new Vector3(-0.74f, 0.82f, 0.55f)),
                (ViewId.Board, new Vector3(-0.7f, 1.52f, -0.25f), new Vector3(-1.75f, 1.5f, -0.25f)),
                (ViewId.Window, new Vector3(0f, 2.25f, -3.4f), new Vector3(0f, -0.9f, -12.9f)),
                (ViewId.Stand, new Vector3(0f, 1.55f, 1.05f), new Vector3(0f, 0.99f, 0.33f)),
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

            // Audio (procedural)
            new GameObject("Audio").AddComponent<ProceduralAudio>();

            // Input and UI roots
            var inputGo = new GameObject("RoomInput");
            var ri = inputGo.AddComponent<RoomInput>();
            ri.cam = cam;
            var uiGo = new GameObject("UI");
            uiGo.AddComponent<TooltipView>();
            uiGo.AddComponent<SubtitleView>();
            uiGo.AddComponent<OverlayView>();
            uiGo.AddComponent<HudView>();
            uiGo.AddComponent<GameFlow>();
            uiGo.AddComponent<ObservationController>();
            UiFactoryEditor.EnsureEventSystem();
            var tm = uiGo.AddComponent<TurnManager>();
            tm.printerSlot = room.printerSlot;
            tm.printerRoot = room.printerRoot;
            tm.standPose = room.standPose;
            tm.tubeMouth = room.tubeMouth;
            tm.trayPoint = room.trayPoint;
            tm.slipParent = room.root;
            tm.printer = room.printer;
            tm.stamps = room.stamps;
            tm.lever = room.lever;
            tm.tube = room.tube;
            tm.paperEdge = Mat("#EEE7D7", 0.95f);
            var mixerView = room.mixer;
            mixerView.cam = cam;

            // Time of day
            var todGo = new GameObject("TimeOfDay");
            var tod = todGo.AddComponent<TimeOfDayController>();
            tod.targetCamera = cam;
            tod.sun = sun;
            tod.windowLight = win;
            tod.streetLights = street.streetLights;
            tod.pendantLight = room.pendantLight;
            tod.deskLamp = room.deskLamp;
            tod.presets.Add(new TimeOfDayController.Preset
            {
                sky = Hex("#BFD9EC"), sun = Hex("#FFF1DC"), sunIntensity = 1.2f, ambient = new Color(0.52f, 0.56f, 0.64f),
                windowIntensity = 0.7f, windowColor = Hex("#CFE3FF"), lampsOn = false, pendantIntensity = 0.45f, lampIntensity = 1.1f,
                shaftOpacity = 0.06f, shaftColor = Color.white
            });
            tod.presets.Add(new TimeOfDayController.Preset
            {
                sky = Hex("#F2B98A"), sun = Hex("#FFB070"), sunIntensity = 1.0f, ambient = new Color(0.50f, 0.42f, 0.38f),
                windowIntensity = 0.55f, windowColor = Hex("#CFE3FF"), lampsOn = false, pendantIntensity = 0.45f, lampIntensity = 1.1f,
                shaftOpacity = 0.09f, shaftColor = Hex("#FFC690")
            });
            tod.presets.Add(new TimeOfDayController.Preset
            {
                sky = Hex("#1D2645"), sun = Hex("#7F95D6"), sunIntensity = 0.25f, ambient = new Color(0.16f, 0.18f, 0.28f),
                windowIntensity = 0.15f, windowColor = Hex("#8FA6FF"), lampsOn = true, pendantIntensity = 1.1f, lampIntensity = 1.6f,
                shaftOpacity = 0.06f, shaftColor = Color.white
            });

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
            // Re-open from disk so the in-memory scene has fresh asset references (imports above can invalidate them).
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[EchoChamber] Built " + ScenePath);
        }

        private static void BuildPostProcessing()
        {
            const string dir = "Assets/_Project/Art/Generated";
            Directory.CreateDirectory(dir);
            string path = dir + "/MainVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            profile.components.Clear();
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.35f);
            bloom.threshold.Override(1.0f);
            bloom.scatter.Override(0.6f);
            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.22f);
            vig.smoothness.Override(0.5f);
            EditorUtility.SetDirty(profile);

            var go = new GameObject("Global Volume");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = profile;
        }
    }
}
