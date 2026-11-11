using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Command line builds. Usage: Unity -batchmode -quit -executeMethod EchoChamber.Editor.BuildScript.BuildWindows -outputPath path/EchoChamber.exe</summary>
    public static class BuildScript
    {
        [MenuItem("Echo Chamber/Build Windows Player")]
        public static void BuildWindows()
        {
            string output = GetArg("-outputPath") ?? "Builds/Windows/EchoChamber.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Main.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("Build result: " + report.summary.result + ", size " + report.summary.totalSize / (1024 * 1024) + " MB");
            if (Application.isBatchMode)
                EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
