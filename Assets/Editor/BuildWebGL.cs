using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace StarIsle.Editor
{
    public static class BuildWebGL
    {
        public static void Build()
        {
            string outDir = "web2";
            if (Directory.Exists(outDir))
            {
                Debug.Log("[BuildWebGL] 清空旧构建目录: " + outDir);
                Directory.Delete(outDir, true);
            }

            var scenes = EditorBuildSettings.scenes;
            string[] scenePaths;
            if (scenes != null && scenes.Length > 0)
                scenePaths = System.Array.ConvertAll(scenes, s => s.path);
            else
                scenePaths = new[] { "Assets/Scenes/SampleScene.unity" };

            var opts = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = outDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log("[BuildWebGL] 开始构建, scenes=" + string.Join(",", scenePaths));
            var report = BuildPipeline.BuildPlayer(opts);
            var s = report.summary;
            if (s.result == BuildResult.Succeeded)
                Debug.Log($"[BuildWebGL] SUCCEEDED totalSize={s.totalSize} bytes");
            else
                Debug.LogError($"[BuildWebGL] FAILED result={s.result} errors={s.totalErrors}");
        }
    }
}
