using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NGCS.MultiBuild
{
    public class PlatformConfig
    {
#if UNITY_EDITOR
        [Title("Platform")] public BuildTarget platform;
#endif
        public bool IsSelected;
        
        [ShowIf("@platform == BuildTarget.Android")]
        public bool buildWithAppBundle;
        
        [ShowIf("@platform == BuildTarget.Android")]
        public bool AutoResolveAndroid;

        [ShowIf("@platform == BuildTarget.Android")]
        public bool exportProject;

        [Title("Version")] public string version;
        public int buildNumber;

        [Title("Configs")] 
        public string DefaultLanguage;
        public string gameName;
        public string companyName;
        public Sprite icon;
        public string packageName;
        public string StoreKey;

        [BoxGroup("Other SDK")] [TitleGroup("Other SDK/GameAnalytics")]
        public bool GameAnalyticsEnable;
        [ShowIf("GameAnalyticsEnable")][BoxGroup("Other SDK")] [TitleGroup("Other SDK/GameAnalytics")]
        public GameAnalyticsConfigModel gameAnalyticsConfig;

        [BoxGroup("Other SDK")] [TitleGroup("Other SDK/Firebase")]
        [InfoBox("Remember to put Firebase google-services files in the Assets folder!", InfoMessageType.Warning,"FirebaseEnable")]
        public bool FirebaseEnable;
        [ShowIf("GameAnalyticsEnable")]
        
        [Title("Define Symbols To Add")] [EnumToggleButtons, GUIColor(.6f, .6f, 1f)]
        public Symbols defineSymbols;

        [Title("Paths")] [FolderPath(ParentFolder = "Assets"), GUIColor(01f, .6f, 0f)] [Space]
        public List<string> includeFolders;

        [Flags]
        public enum Symbols
        {
            ZarinPalPayment = 1 << 1
        }
        
        public List<string> GetDefineSymbols()
        {
            var symbols = defineSymbols.ToString() == "0" ? new List<string>() : defineSymbols.ToString().Split(',').ToList();
            if(GameAnalyticsEnable)
                symbols.Add("GameAnalyticsEnable");
            if(FirebaseEnable)
                symbols.Add("FirebaseEnable");
            return symbols;
        }
    }
}