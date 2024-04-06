#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NGCS.MultiBuild
{
    public class Builder : MonoBehaviour
    {
        public static Action<MarketType> onBuildComplete;

        public static void Build(string[] scenes, PlatformConfig config, MarketType marketType, BuildType buildType,
            string buildPath)
        {
            var bundleBuild = config.platform == BuildTarget.Android && config.buildWithAppBundle;
            if (config.platform == BuildTarget.Android)
                EditorUserBuildSettings.exportAsGoogleAndroidProject = config.exportProject;

            EditorUserBuildSettings.buildAppBundle = bundleBuild;
            var postfix = config.platform == BuildTarget.Android ? bundleBuild ? ".aab" : ".apk" : "";
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = GetLocationPathName(config, marketType, buildType, buildPath) + postfix,
                target = config.platform,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log($"<color=green>Build <color=yellow>{marketType}-{buildType}</color> succeeded.</color>");
                    onBuildComplete?.Invoke(marketType);
                    EditorUtility.RevealInFinder(summary.outputPath);
                    break;
                case BuildResult.Failed:
                    Debug.Log($"<color=red>Build <color=yellow>{marketType}-{buildType}</color> failed.</color>");
                    break;
            }
        }

        private static string GetLocationPathName(PlatformConfig config, MarketType marketType, BuildType buildType,
            string buildPath)
        {
            return $"{buildPath}/v{config.version}-Build{config.buildNumber}-{buildType}-{marketType}";
        }
    }
}
#endif