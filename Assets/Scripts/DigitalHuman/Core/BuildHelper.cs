using UnityEngine;
using UnityEditor;
using System.Linq;

namespace DigitalHuman.Core
{
    public static class BuildHelper
    {
        public static void Build()
        {
            // Get enabled scenes from Build Settings
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
                
            // If no scenes in build settings, use the current open scene or a default
            if (scenes.Length == 0)
            {
                // Fallback to finding all scenes in Assets
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                if (guids.Length > 0)
                {
                    scenes = new[] { AssetDatabase.GUIDToAssetPath(guids[0]) };
                }
            }

            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = "Builds/TestBuild.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.Development; 

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("Build Succeeded");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("Build Failed");
                EditorApplication.Exit(1);
            }
        }
    }
}
