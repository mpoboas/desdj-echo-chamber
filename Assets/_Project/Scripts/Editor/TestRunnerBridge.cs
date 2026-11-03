using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace EchoChamber.Editor
{
    /// <summary>Runs the project EditMode tests from a menu item and writes a summary to Logs/test-results.txt.</summary>
    public static class TestRunnerBridge
    {
        public static string ResultPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs", "test-results.txt");

        [MenuItem("Echo Chamber/Run EditMode Tests")]
        public static void Run()
        {
            File.WriteAllText(ResultPath, "RUNNING\n");
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Collector());
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "EchoChamber.Tests" }
            }));
        }

        private class Collector : ICallbacks
        {
            private readonly StringBuilder failures = new StringBuilder();

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.Test.IsSuite) return;
                if (result.TestStatus == TestStatus.Failed)
                {
                    failures.AppendLine("FAIL " + result.Test.FullName);
                    failures.AppendLine("  " + (result.Message ?? "").Replace("\n", "\n  "));
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var sb = new StringBuilder();
                sb.AppendLine("DONE");
                sb.AppendLine($"passed={result.PassCount} failed={result.FailCount} skipped={result.SkipCount} inconclusive={result.InconclusiveCount}");
                sb.Append(failures);
                File.WriteAllText(ResultPath, sb.ToString());
            }
        }
    }
}
