using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IngameDebugConsole;
using Newtonsoft.Json;
using NGCS.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace NGCS.MultiBuild
{
    [CreateAssetMenu(fileName = "MultiBuildConfigs", menuName = "Incytel/Create Build Config")]
    public class MultiBuildConfig : SerializedScriptableObject, IMultiBuildConfig
    {
        public KeyValuePair<MarketType, PlatformConfig> CurrentConfig =>
            PlatformConfigs.FirstOrDefault(p => p.Value.IsSelected);

#if UNITY_EDITOR

        #region ******** Configs ********

        private string GamePath => Application.dataPath.Replace("/Assets", "");

        private static string ResolveAfterCompileKey => "Resolve after compile";
        private string buildPath => GamePath + "/Builds";
        private string baseAndroidManifestPath => GamePath + baseAndroidPath + "/AndroidManifest.xml";
        private string baseAndroidPath => GamePath + "/Plugins/Android";


        [TitleGroup("Platforms")] [PropertyOrder(-3)] [EnumToggleButtons, GUIColor(1f, .6f, 0f)]
        public MarketType marketType;

        [TitleGroup("Platforms")] [PropertyOrder(-3)] [EnumToggleButtons, GUIColor(1f, .6f, 0.5f)]
        public BuildType buildType;

        [TitleGroup("Server Configs"), GUIColor(1, 1, .7f)]
        public ServerConfig baseServerConfig;

        [TitleGroup("Server Configs"), GUIColor(1, 1, .7f)]
        public Dictionary<BuildType, ServerConfig> ServerConfigs;

        [TitleGroup("Configs"), GUIColor(1, 1, .7f)]
        public GameObject InGameDebugConsole;

        [DictionaryDrawerSettings(KeyLabel = "Custom Key Name", ValueLabel = "Custom Value Label"), GUIColor(1, 1, .7f)]
        [TitleGroup("Configs")]
        public Dictionary<MarketType, PlatformConfig> PlatformConfigs = new();

        #endregion


        #region ******** Build ********

        [ButtonGroup("Platforms/Buttons")]
        [PropertyOrder(-2)]
        [Button(ButtonSizes.Large), GUIColor(.3f, 1, .6f)]
        public void BuildThisPlatform()
        {
            Builder.Build(
                EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                GetCurrentConfig(), marketType, buildType, buildPath);
        }

        [ButtonGroup("Platforms/Buttons")]
        [PropertyOrder(-2)]
        [Button(ButtonSizes.Large), GUIColor(1f, 1, .3f)]
        public void SetupForThisBuild()
        {
            SelectPlatform();
            SwitchPlatform(GetCurrentConfig().platform);
            HandleSymbols();
            HandleSeverConfig();
            HandleAndroidManifest();
            ShowThisPlatformFolder();
            SetIcon(marketType);
            SetupCurrentConfig();
            SetupAnalytics();
            SetupDebugConsole();

            // Save resolving state
            PlayerPrefs.SetInt(ResolveAfterCompileKey, GetCurrentConfig().AutoResolveAndroid ? 1 : 0);
        }

        private void SelectPlatform()
        {
            foreach (var config in PlatformConfigs)
            {
                config.Value.IsSelected = config.Key == marketType;
            }
        }

        [Button]
        private void SetupDebugConsole()
        {
            if (InGameDebugConsole == null) return;
            if (buildType == BuildType.Production)
            {
                var console = FindObjectOfType<DebugLogManager>(true);
                if (console == null) return;
                console.gameObject.SetActive(false);
                EditorUtility.SetDirty(console);
            }
            else
            {
                var console = FindObjectOfType<DebugLogManager>(true);
                if (console != null)
                {
                    console.gameObject.SetActive(true);
                    return;
                }

                PrefabUtility.InstantiatePrefab(InGameDebugConsole);
            }
        }

        [ButtonGroup("Platforms/Buttons")]
        [PropertyOrder(-2)]
        [Button(ButtonSizes.Large), GUIColor(.6f, 0.6f, 1f)]
        public static void Resolve()
        {
            EditorApplication.ExecuteMenuItem(
                "Assets/External Dependency Manager/Android Resolver/Force Resolve");
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AfterCompile()
        {
            if (PlayerPrefs.GetInt(ResolveAfterCompileKey, 0) == 0) return;
            Resolve();
            PlayerPrefs.SetInt(ResolveAfterCompileKey, 0);
        }

        #endregion

        #region ******** Actions ********

        // HandleSymbols function to manage platform and market symbols
        private void HandleSymbols()
        {
            // Get all platform and market symbols
            var allPlatformsSymbols = Enum.GetNames(typeof(PlatformConfig.Symbols))
                .Concat(Enum.GetNames(typeof(MarketType))).Concat(new[] { "FirebaseEnable", "GameAnalyticsEnable" })
                .ToList();

            // Get current platform symbols and add market type symbol
            var currentPlatformSymbols = GetCurrentConfig().GetDefineSymbols();
            currentPlatformSymbols.Add(marketType.ToString());

            // Get all script defines for the selected build target group
            var allDefines = PlayerSettings
                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
                .Split(';').ToList();

            // Calculate original symbols by excluding platform symbols and adding current platform symbols
            var originalSymbols = allDefines.Except(allPlatformsSymbols).ToList();
            originalSymbols.AddRange(currentPlatformSymbols.Except(originalSymbols));

            // Set the scripting define symbols for the selected build target group
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", originalSymbols.ToArray()));
        }

        private void HandleSeverConfig()
        {
            ReplaceServerConfig(ServerConfigs[buildType]);
        }

        private void ReplaceServerConfig(ServerConfig currentServerConfig)
        {
            if (currentServerConfig == null)
            {
                NLogger.Log("Server config not found");
            }

            baseServerConfig.BaseUrl = currentServerConfig.BaseUrl;
            baseServerConfig.RoomUrl = currentServerConfig.RoomUrl;
            baseServerConfig.GameCenterUrl = currentServerConfig.GameCenterUrl;

            EditorUtility.SetDirty(baseServerConfig);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region ******** Switch Platform ********

        private void SwitchPlatform(BuildTarget buildTarget)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
        }

        #endregion

        #region ******** Folders ********

        private void ShowThisPlatformFolder()
        {
            var hiddenFolders = new List<string>();
            foreach (var configsValue in PlatformConfigs.Values.Where(configsValue =>
                         configsValue?.includeFolders != null))
            {
                hiddenFolders.AddRange((configsValue?.includeFolders).Where(includeFolder =>
                    !GetCurrentConfig().includeFolders.Exists(f => f == includeFolder)));
            }

            RenameToNormalPath(GetCurrentConfig().includeFolders);
            RenameToHiddenPath(hiddenFolders);
        }

        private void RenameToNormalPath(List<string> includeFolders)
        {
            if (includeFolders.IsNullOrEmpty()) return;
            foreach (var folder in includeFolders)
                MovePath(folder + "~", folder);
        }

        private void RenameToHiddenPath(List<string> excludeFolders)
        {
            if (excludeFolders.IsNullOrEmpty()) return;

            foreach (var folder in excludeFolders)
                MovePath(folder, folder + "~");
        }

        private void MovePath(string oldPath, string newPath)
        {
            oldPath = Path.Combine(Application.dataPath, oldPath);
            newPath = Path.Combine(Application.dataPath, newPath);

            if (!Directory.Exists(oldPath)) return;
            MoveDirectory($@"{oldPath}", $@"{newPath}");
            AssetDatabase.Refresh();
        }

        private void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(Path.GetDirectoryName);
            foreach (var folder in files)
            {
                var targetFolder = Path.Combine(targetPath, folder.Key.Substring(sourcePath.Length));

                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }

        #endregion

        #region ******** Set configs ********

        private void SetupCurrentConfig()
        {
            PlayerSettings.productName = GetCurrentConfig().gameName;
            PlayerSettings.companyName = GetCurrentConfig().companyName;
            PlayerSettings.bundleVersion = GetCurrentConfig().version;
            if (GetCurrentConfig().platform == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = GetCurrentConfig().buildNumber;
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, GetCurrentConfig().packageName);
            }
            else
            {
                PlayerSettings.iOS.buildNumber = GetCurrentConfig().buildNumber.ToString();
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, GetCurrentConfig().packageName);
            }
        }

        private void HandleAndroidManifest()
        {
            if (GetCurrentConfig().platform != BuildTarget.Android) return;

            var platformManifestPath =
                $"{GamePath}//InAppPurchase/Vendors SDK/{GetCurrentMarketType()}/AndroidManifest.xml";

            ReplaceFile(baseAndroidManifestPath, platformManifestPath);
        }

        private void ReplaceFile(string oldPath, string newPath)
        {
            var isFileExist = File.Exists(oldPath);
            if (isFileExist)
                File.Delete(oldPath);

            isFileExist = File.Exists(oldPath);
            if (!isFileExist) return;

            File.Copy(newPath, oldPath);
        }


        private void SetIcon(MarketType buildType)
        {
            if (PlatformConfigs[buildType].icon == null)
            {
                Debug.Log($"<color=orange>⚠️ icon not set for {buildType}</color> ");
                return;
            }

            var target = PlatformConfigs[buildType].platform == BuildTarget.Android
                ? BuildTargetGroup.Android
                : BuildTargetGroup.iOS;

            var icons = new Texture2D[PlayerSettings.GetIconSizesForTargetGroup(target).Length];
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = PlatformConfigs[buildType].icon.texture;
            }

            PlayerSettings.SetIconsForTargetGroup(target, icons);
        }

        private void SetupAnalytics()
        {
            BuildAnalyticsHandler.SetupAnalytics(GetCurrentConfig());
        }

        #endregion

#endif //End of UNITY_EDITOR condition

        public MarketType GetCurrentMarketType()
        {
            return CurrentConfig.Key;
        }

        public PlatformConfig GetCurrentConfig()
        {
            return CurrentConfig.Value;
        }
    }
}