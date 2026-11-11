using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace EchoChamber
{
    /// <summary>
    /// The TextDepth shader is not given the dynamic font atlas by TextMesh, so bind it per renderer
    /// (MaterialPropertyBlock, never serialized) at load and whenever the atlas is rebuilt.
    /// </summary>
    public static class TextDepthBinder
    {
        static bool hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnRuntimeLoad() { Hook(); Apply(); }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void OnEditorLoad()
        {
            Hook();
            EditorApplication.delayCall += Apply;
            EditorSceneManager.sceneOpened += (s, m) => Apply();
            EditorApplication.playModeStateChanged += _ => EditorApplication.delayCall += Apply;
        }
#endif

        static void Hook()
        {
            if (hooked) return;
            hooked = true;
            Font.textureRebuilt += _ => Apply();
        }

        public static void Apply()
        {
            var mpb = new MaterialPropertyBlock();
            foreach (var tm in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
            {
                if (tm == null || tm.font == null) continue;
                var r = tm.GetComponent<MeshRenderer>();
                if (r == null || r.sharedMaterial == null || r.sharedMaterial.shader.name != "EchoChamber/TextDepth") continue;
                r.GetPropertyBlock(mpb);
                mpb.SetTexture("_MainTex", tm.font.material.mainTexture);
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
