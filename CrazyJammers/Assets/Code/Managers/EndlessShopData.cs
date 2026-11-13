using System.Collections.Generic;
using UnityEngine;

public enum EndlessItemEffectType
{
    FullHealAndCure, // Miracle Dew
    FreeItemAction,  // Blessed Amulet
    Placeholder      // Future Excel sheet items
}

[System.Serializable]
public class EndlessItemDefinition
{
    public string itemId;
    public string displayName;
    public int price;
    public int maxCarryLimit = 99;
    public EndlessItemEffectType effectType;
    [TextArea(2, 4)] public string description;
    public string emptyDescription = "None left in this run.";
}

public static class EndlessShopData
{
    private const string GALD_KEY = "PlayerTotalGald";
    private const string SHOP_UNLOCKED_KEY = "ShopUnlocked";
    private const string FIRST_SHOP_NOTIF_SHOWN_KEY = "FirstShopNotifShown";

    public const string MIRACLE_DEW_ID = "MiracleDew";
    public const string BLESSED_AMULET_ID = "BlessedAmulet";

    // Central Catalog: Add future Excel items right here!
    public static readonly List<EndlessItemDefinition> Catalog = new List<EndlessItemDefinition>()
    {
        new EndlessItemDefinition
        {
            itemId = MIRACLE_DEW_ID,
            displayName = "Miracle Dew",
            price = 800,
            maxCarryLimit = 99,
            effectType = EndlessItemEffectType.FullHealAndCure,
            description = "Restores full HP and cures all status effects.",
            emptyDescription = "No Miracle Dew left."
        },
        new EndlessItemDefinition
        {
            itemId = BLESSED_AMULET_ID,
            displayName = "Blessed Amulet",
            price = 15000,
            maxCarryLimit = 10,
            effectType = EndlessItemEffectType.FreeItemAction,
            description = "Allows you to use an item and Bide or Attack in 1 turn.",
            emptyDescription = "No Blessed Amulet left."
        }
    };

    public static int TotalGald
    {
        get => PlayerPrefs.GetInt(GALD_KEY, 0);
        set { PlayerPrefs.SetInt(GALD_KEY, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static bool IsShopUnlocked
    {
        get => PlayerPrefs.GetInt(SHOP_UNLOCKED_KEY, 0) == 1;
        set { PlayerPrefs.SetInt(SHOP_UNLOCKED_KEY, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static bool HasShownShopUnlockNotif
    {
        get => PlayerPrefs.GetInt(FIRST_SHOP_NOTIF_SHOWN_KEY, 0) == 1;
        set { PlayerPrefs.SetInt(FIRST_SHOP_NOTIF_SHOWN_KEY, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void AddGald(int amount)
    {
        if (amount > 0) TotalGald += amount;
    }

    public static int GetPermanentItemCount(string itemId) => PlayerPrefs.GetInt($"ShopItem_{itemId}", 0);

    public static void AddPermanentItem(string itemId, int amount)
    {
        int current = GetPermanentItemCount(itemId);
        PlayerPrefs.SetInt($"ShopItem_{itemId}", Mathf.Clamp(current + amount, 0, 99));
        PlayerPrefs.Save();
    }
}