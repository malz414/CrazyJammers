#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DebugTools
{
    private const string EndlessPrefKey = "EndlessModeUnlocked";
    private const string ShopPrefKey = "ShopUnlocked";
    private const string ShopNotifPrefKey = "FirstShopNotifShown";

    [MenuItem("Debug Tools/Endless Mode/Lock (Disable)")]
    public static void LockEndlessMode()
    {
        PlayerPrefs.SetInt(EndlessPrefKey, 0);
        PlayerPrefs.Save();
        Debug.Log("[DebugTools] Endless Mode LOCKED (Disabled).");
    }

    [MenuItem("Debug Tools/Endless Mode/Unlock (Enable)")]
    public static void UnlockEndlessMode()
    {
        PlayerPrefs.SetInt(EndlessPrefKey, 1);
        PlayerPrefs.Save();
        Debug.Log("[DebugTools] Endless Mode UNLOCKED (Enabled).");
    }

    [MenuItem("Debug Tools/Shop Mode/Lock (Disable)")]
    public static void LockShopMode()
    {
        PlayerPrefs.SetInt(ShopPrefKey, 0);
        PlayerPrefs.SetInt(ShopNotifPrefKey, 0);
        PlayerPrefs.Save();
        Debug.Log("[DebugTools] Shop Mode LOCKED (Disabled).");
    }

    [MenuItem("Debug Tools/Shop Mode/Unlock (Enable)")]
    public static void UnlockShopMode()
    {
        PlayerPrefs.SetInt(ShopPrefKey, 1);
        PlayerPrefs.Save();
        Debug.Log("[DebugTools] Shop Mode UNLOCKED (Enabled). Note: The popup will trigger on next scene load.");
    }

    [MenuItem("Debug Tools/Wipe ALL Save Data (Hard Reset)")]
    public static void WipeAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[DebugTools] ALL PlayerPrefs have been completely wiped. Game is reset to factory settings.");
    }
}
#endif