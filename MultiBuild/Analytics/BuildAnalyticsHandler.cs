#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NGCS.MultiBuild
{
    public static class BuildAnalyticsHandler
    {
        public static void SetupAnalytics(PlatformConfig currentConfig)
        {
            SetupGameAnalytics(currentConfig.gameAnalyticsConfig,
                currentConfig.platform == BuildTarget.Android ? RuntimePlatform.Android : RuntimePlatform.IPhonePlayer);
            AssetDatabase.SaveAssets();
        }

        private static void SetupGameAnalytics(GameAnalyticsConfigModel configModel, RuntimePlatform platform)
        {
#if GameAnalyticsEnable
            if (string.IsNullOrEmpty(configModel.GameKey) || string.IsNullOrEmpty(configModel.SecretKey))
                return;

            if (configModel.GameKey.ToLower() == "ignore" && configModel.SecretKey.ToLower() == "ignore")
                return;

            var setting = GameAnalyticsSDK.GameAnalytics.SettingsGA;
            if (!setting.Platforms.Contains(platform))
                setting.AddPlatform(platform);

            int platformIndex = setting.Platforms.IndexOf(platform);
            setting.UpdateGameKey(platformIndex, configModel.GameKey);
            setting.UpdateSecretKey(platformIndex, configModel.SecretKey);
            setting.Build[platformIndex] = Application.version;
            setting.InfoLogBuild = false;
            setting.InfoLogEditor = false;
            EditorUtility.SetDirty(setting);
#endif
        }
    }
}

#endif