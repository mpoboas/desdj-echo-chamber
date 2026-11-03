using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Mirrors the Unity console to Logs/console.txt so it can be read from outside the Editor.</summary>
    [InitializeOnLoad]
    public static class ConsoleCapture
    {
        private static readonly string Path_ = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "Logs", "console.txt");
        private static readonly object Lock = new object();

        static ConsoleCapture()
        {
            Application.logMessageReceivedThreaded -= OnLog;
            Application.logMessageReceivedThreaded += OnLog;
        }

        [MenuItem("Echo Chamber/Clear Captured Console")]
        public static void Clear()
        {
            lock (Lock) File.WriteAllText(Path_, "");
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (condition != null && (condition.Contains("SuperProxy") || condition.Contains("[relay]"))) return;
            string line = $"[{System.DateTime.Now:HH:mm:ss}] {type}: {condition}";
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                string st = stackTrace ?? "";
                int cut = st.Length > 600 ? 600 : st.Length;
                line += "\n" + st.Substring(0, cut);
            }
            lock (Lock) File.AppendAllText(Path_, line + "\n");
        }
    }
}
