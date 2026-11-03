using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>The street is tiny (people are 0.66 tall), so shrink the default humanoid NavMesh agent type.</summary>
    public static class NavMeshSettingsEditor
    {
        public static void EnsureSmallAgent()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[EchoChamber] NavMeshAreas.asset not found");
                return;
            }
            var so = new SerializedObject(assets[0]);
            var settings = so.FindProperty("m_Settings");
            if (settings == null || settings.arraySize == 0) return;
            var s = settings.GetArrayElementAtIndex(0);
            s.FindPropertyRelative("agentRadius").floatValue = 0.07f;
            s.FindPropertyRelative("agentHeight").floatValue = 0.62f;
            s.FindPropertyRelative("agentSlope").floatValue = 60f;
            s.FindPropertyRelative("agentClimb").floatValue = 0.12f;
            so.ApplyModifiedProperties();
        }
    }
}
