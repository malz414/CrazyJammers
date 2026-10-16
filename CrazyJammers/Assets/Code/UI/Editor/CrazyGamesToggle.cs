using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CrazyGamesToggleTool
{
    private const string DEFINE_SYMBOL = "USE_CRAZYGAMES";

    [MenuItem("Tools/CrazyGames SDK/Enable SDK", false, 1)]
    public static void EnableSDK()
    {
        SetDefineSymbol(true);
    }

    [MenuItem("Tools/CrazyGames SDK/Disable SDK", false, 2)]
    public static void DisableSDK()
    {
        SetDefineSymbol(false);
    }

    // Optional: Adds a checkmark in the menu to show current status
    [MenuItem("Tools/CrazyGames SDK/Enable SDK", true)]
    public static bool EnableSDKValidate()
    {
        Menu.SetChecked("Tools/CrazyGames SDK/Enable SDK", IsSymbolDefined());
        return true;
    }

    private static bool IsSymbolDefined()
    {
        NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        return defines.Split(';').Contains(DEFINE_SYMBOL);
    }

    private static void SetDefineSymbol(bool enable)
    {
        NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        List<string> defineList = defines.Split(';').ToList();

        if (enable && !defineList.Contains(DEFINE_SYMBOL))
        {
            defineList.Add(DEFINE_SYMBOL);
            Debug.Log("CrazyGames SDK Enabled. Recompiling scripts...");
        }
        else if (!enable && defineList.Contains(DEFINE_SYMBOL))
        {
            defineList.Remove(DEFINE_SYMBOL);
            Debug.Log("CrazyGames SDK Disabled. Recompiling scripts...");
        }

        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, string.Join(";", defineList));
    }
}