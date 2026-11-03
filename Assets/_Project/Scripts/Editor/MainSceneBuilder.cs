using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
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
            host.AddComponent<DebugAutoplay>();

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
            camGo.transform.position = P(0f, 2.25f, -3.4f);
            camGo.transform.LookAt(P(0f, -0.9f, -12.9f));

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
            var street = StreetBuilder.Build(content.content, tuning.data);

            // Time of day
            var todGo = new GameObject("TimeOfDay");
            var tod = todGo.AddComponent<TimeOfDayController>();
            tod.targetCamera = cam;
            tod.sun = sun;
            tod.windowLight = win;
            tod.streetLights = street.streetLights;
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
