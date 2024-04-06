using System.IO;
using UnityEditor;
using UnityEngine;

namespace NGCS.MultiBuild
{
    public class MultiBuildEditorUtils
    {
        private const string ConfigName = "MultiBuildConfigs";

        [MenuItem("Incytel/Show MultiBuild Config")]
        public static void ShowFontAssets()
        {
            var configTemplate = Resources.Load(ConfigName);
            if (configTemplate == null)
            {
                configTemplate = ScriptableObject.CreateInstance<MultiBuildConfig>();
                AssetDatabase.CreateAsset(configTemplate, $"Assets/Resources/{ConfigName}.asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
            }

            Selection.activeObject = configTemplate;
        }
    }
}