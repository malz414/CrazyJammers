#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class DebugTools
{
    private const string EndlessPrefKey = "EndlessModeUnlocked";

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

    [MenuItem("Debug Tools/Endless Mode/Toggle")]
    public static void ToggleEndlessMode()
    {
        int current = PlayerPrefs.GetInt(EndlessPrefKey, 0);
        int newState = current == 1 ? 0 : 1;
        PlayerPrefs.SetInt(EndlessPrefKey, newState);
        PlayerPrefs.Save();
        Debug.Log($"[DebugTools] Endless Mode toggled to: {(newState == 1 ? "UNLOCKED" : "LOCKED")}");
    }
}
#endif