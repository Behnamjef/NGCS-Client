#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

public static class ScriptingDefineSymbols
{
    public static void AddThisToSymbols(string[] Symbols)
    {
        var definesString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(GetTargetGroup());

        var allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(Symbols.Except(allDefines));

        definesString = string.Join(";", allDefines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTargetGroup(),
            definesString);

        AssetDatabase.SaveAssets();
    }

    public static void RemoveTheseSymbols(string[] Symbols)
    {
        var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetTargetGroup());

        var allDefines = definesString.Split(';').ToList();
        for (var i = 0; i < allDefines.Count; i++)
        {
            if (Symbols.Contains(allDefines[i]))
            {
                allDefines.RemoveAt(i);
            }
        }

        definesString = string.Join(";", allDefines.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTargetGroup(), definesString);

        AssetDatabase.SaveAssets();
    }

    private static BuildTargetGroup GetTargetGroup()
    {
        var targetGroup = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
            ? BuildTargetGroup.Android
            : BuildTargetGroup.iOS;
        return targetGroup;
    }
}
#endif