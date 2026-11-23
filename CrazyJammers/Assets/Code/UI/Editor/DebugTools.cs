#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#if USE_CRAZYGAMES
using CrazyGames;
#endif

public static class DebugTools
{
    [MenuItem("Debug Tools/Endless Mode/Lock (Disable)")]
    public static void LockEndlessMode()
    {
        EndlessShopData.IsEndlessUnlocked = false;
        EndlessShopData.HasShownEndlessUnlockNotif = false;
        Debug.Log("[DebugTools] Endless Mode LOCKED (Disabled).");
    }

    [MenuItem("Debug Tools/Endless Mode/Unlock (Enable & Reset Popup)")]
    public static void UnlockEndlessMode()
    {
        EndlessShopData.IsEndlessUnlocked = true;
        EndlessShopData.HasShownEndlessUnlockNotif = false;
        Debug.Log("[DebugTools] Endless Mode UNLOCKED and popup flag reset.");
    }

    [MenuItem("Debug Tools/Shop Mode/Lock (Disable)")]
    public static void LockShopMode()
    {
        EndlessShopData.IsShopUnlocked = false;
        EndlessShopData.HasShownShopUnlockNotif = false;
        Debug.Log("[DebugTools] Shop Mode LOCKED (Disabled).");
    }

    [MenuItem("Debug Tools/Shop Mode/Unlock (Enable & Reset Popup)")]
    public static void UnlockShopMode()
    {
        EndlessShopData.IsShopUnlocked = true;
        EndlessShopData.HasShownShopUnlockNotif = false;
        Debug.Log("[DebugTools] Shop Mode UNLOCKED and popup flag reset.");
    }

    [MenuItem("Debug Tools/Wipe ALL Save Data (Hard Reset)")]
    public static void WipeAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.DeleteKey("EndlessModeUnlocked");
        PlayerPrefs.DeleteKey("FirstEndlessNotifShown");
        PlayerPrefs.DeleteKey("ShopUnlocked");
        PlayerPrefs.DeleteKey("FirstShopNotifShown");
        PlayerPrefs.DeleteKey("SavedLevel");
        PlayerPrefs.Save();

#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Data.DeleteAll();
        }
#endif

        Debug.Log($"[DebugTools] Hard Reset Complete -> IsEndlessUnlocked={EndlessShopData.IsEndlessUnlocked} | HasShownEndlessNotif={EndlessShopData.HasShownEndlessUnlockNotif} | IsShopUnlocked={EndlessShopData.IsShopUnlocked}");
    }
}
#endif