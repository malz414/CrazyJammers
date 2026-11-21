using System.Collections.Generic;
using UnityEngine;

public enum EndlessItemEffectType
{
    HealPercent,        // Potion (20%), Great Potion (45%), Mega Potion (75%)
    CureStatus,         // Panacea
    HealPercentAndCure, // Elixir (20%), Great Elixir (50%), Miracle Dew (100%)
    FreeItemAction      // Blessed Amulet
}

[System.Serializable]
public class EndlessItemDefinition
{
    public string itemId;
    public string displayName;
    public int price;
    public int maxCarryLimit = 99;
    public bool soldInShop = true;
    public int endlessBaseCount = 0; // Guaranteed starting amount every Endless run (before shop purchases)
    public float healPercent = 0f;   // 0.20f = 20%, 1.0f = 100%
    public EndlessItemEffectType effectType;
    [TextArea(2, 4)] public string description;
    public string emptyDescription = "None left.";
}

public struct MatchRewardEntry
{
    public string itemId;
    public int amount;

    public MatchRewardEntry(string id, int count)
    {
        itemId = id;
        amount = count;
    }
}

public static class EndlessShopData
{
    private const string GALD_KEY = "PlayerTotalGald";
    private const string SHOP_UNLOCKED_KEY = "ShopUnlocked";
    private const string FIRST_SHOP_NOTIF_SHOWN_KEY = "FirstShopNotifShown";

    public const string POTION_ID = "Potion";
    public const string GREAT_POTION_ID = "GreatPotion";
    public const string MEGA_POTION_ID = "MegaPotion";
    public const string PANACEA_ID = "Panacea";
    public const string ELIXIR_ID = "Elixir";
    public const string GREAT_ELIXIR_ID = "GreatElixir";
    public const string MIRACLE_DEW_ID = "MiracleDew";
    public const string BLESSED_AMULET_ID = "BlessedAmulet";

    // Unified Item Catalog (Prices for Mega Potion, Elixir, and Great Elixir are placeholders — adjust once finalized)
    public static readonly List<EndlessItemDefinition> Catalog = new List<EndlessItemDefinition>()
    {
        new EndlessItemDefinition
        {
            itemId = POTION_ID,
            displayName = "Potion",
            price = 0,
            maxCarryLimit = 99,
            soldInShop = false,
            endlessBaseCount = 1,
            healPercent = 0.20f,
            effectType = EndlessItemEffectType.HealPercent,
            description = "Restores 20% of max HP.",
            emptyDescription = "No Potion left."
        },
        new EndlessItemDefinition
        {
            itemId = GREAT_POTION_ID,
            displayName = "Great Potion",
            price = 0,
            maxCarryLimit = 99,
            soldInShop = false,
            endlessBaseCount = 0,
            healPercent = 0.45f,
            effectType = EndlessItemEffectType.HealPercent,
            description = "Restores 45% of max HP.",
            emptyDescription = "No Great Potion left."
        },
        new EndlessItemDefinition
        {
            itemId = MEGA_POTION_ID,
            displayName = "Mega Potion",
            price = 400, // Placeholder price
            maxCarryLimit = 99,
            soldInShop = true,
            endlessBaseCount = 3, // Match 5 reward
            healPercent = 0.75f,
            effectType = EndlessItemEffectType.HealPercent,
            description = "Restores 75% of max HP.",
            emptyDescription = "No Mega Potion left."
        },
        new EndlessItemDefinition
        {
            itemId = PANACEA_ID,
            displayName = "Panacea",
            price = 0,
            maxCarryLimit = 99,
            soldInShop = false,
            endlessBaseCount = 2,
            healPercent = 0f,
            effectType = EndlessItemEffectType.CureStatus,
            description = "Cures all status effects.",
            emptyDescription = "No Panacea left."
        },
        new EndlessItemDefinition
        {
            itemId = ELIXIR_ID,
            displayName = "Elixir",
            price = 300, // Placeholder price
            maxCarryLimit = 99,
            soldInShop = true,
            endlessBaseCount = 0,
            healPercent = 0.20f,
            effectType = EndlessItemEffectType.HealPercentAndCure,
            description = "Cures all status effects and restores 20% HP.",
            emptyDescription = "No Elixir left."
        },
        new EndlessItemDefinition
        {
            itemId = GREAT_ELIXIR_ID,
            displayName = "Great Elixir",
            price = 550, // Placeholder price
            maxCarryLimit = 99,
            soldInShop = true,
            endlessBaseCount = 2, // Match 5 reward
            healPercent = 0.50f,
            effectType = EndlessItemEffectType.HealPercentAndCure,
            description = "Cures all status effects and restores 50% HP.",
            emptyDescription = "No Great Elixir left."
        },
        new EndlessItemDefinition
        {
            itemId = MIRACLE_DEW_ID,
            displayName = "Miracle Dew",
            price = 800,
            maxCarryLimit = 99,
            soldInShop = true,
            endlessBaseCount = 1, // Match 5 reward
            healPercent = 1.0f,
            effectType = EndlessItemEffectType.HealPercentAndCure,
            description = "Restores full HP and cures all status effects.",
            emptyDescription = "No Miracle Dew left."
        },
        new EndlessItemDefinition
        {
            itemId = BLESSED_AMULET_ID,
            displayName = "Blessed Amulet",
            price = 15000,
            maxCarryLimit = 10,
            soldInShop = true,
            endlessBaseCount = 0,
            healPercent = 0f,
            effectType = EndlessItemEffectType.FreeItemAction,
            description = "Allows you to use an item and Bide or Attack in 1 turn.",
            emptyDescription = "No Blessed Amulet left."
        }
    };

    public static EndlessItemDefinition GetDefinition(string itemId)
    {
        return Catalog.Find(i => i.itemId == itemId);
    }

    public static List<EndlessItemDefinition> GetShopCatalog()
    {
        return Catalog.FindAll(i => i.soldInShop);
    }

    // --- GALD & SHOP UNLOCK ---
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

    // --- ENDLESS PERMANENT INVENTORY ---
    public static int GetPermanentItemCount(string itemId) => PlayerPrefs.GetInt($"ShopItem_{itemId}", 0);

    // Total starting stock in Endless Mode = Base Endless Grant (Potion/Panacea/Match 5) + Shop Purchases
    public static int GetTotalEndlessOwned(string itemId)
    {
        var def = GetDefinition(itemId);
        if (def == null) return 0;
        return Mathf.Clamp(def.endlessBaseCount + GetPermanentItemCount(itemId), 0, def.maxCarryLimit);
    }

    public static void AddPermanentItem(string itemId, int amount)
    {
        var def = GetDefinition(itemId);
        int limit = def != null ? def.maxCarryLimit : 99;
        int baseCount = def != null ? def.endlessBaseCount : 0;
        int currentBought = GetPermanentItemCount(itemId);
        int maxBuyable = Mathf.Max(0, limit - baseCount);

        PlayerPrefs.SetInt($"ShopItem_{itemId}", Mathf.Clamp(currentBought + amount, 0, maxBuyable));
        PlayerPrefs.Save();
    }

    // --- MAIN CAMPAIGN (MATCHES 1-5) INVENTORY & REWARDS ---
    private static Dictionary<string, int> campaignCheckpoint = new Dictionary<string, int>();

    public static void ResetCampaignInventory()
    {
        foreach (var item in Catalog)
        {
            PlayerPrefs.SetInt($"CampaignItem_{item.itemId}", 0);
        }
        // Match 1 Pre-Match Inventory
        PlayerPrefs.SetInt($"CampaignItem_{POTION_ID}", 1);
        PlayerPrefs.SetInt($"CampaignItem_{PANACEA_ID}", 1);
        PlayerPrefs.Save();
    }

    public static int GetCampaignItemCount(string itemId) => PlayerPrefs.GetInt($"CampaignItem_{itemId}", 0);

    public static void SetCampaignItemCount(string itemId, int count)
    {
        var def = GetDefinition(itemId);
        int limit = def != null ? def.maxCarryLimit : 99;
        PlayerPrefs.SetInt($"CampaignItem_{itemId}", Mathf.Clamp(count, 0, limit));
        PlayerPrefs.Save();
    }

    public static void AddCampaignItem(string itemId, int amount)
    {
        SetCampaignItemCount(itemId, GetCampaignItemCount(itemId) + amount);
    }

    public static void SaveCampaignCheckpoint()
    {
        campaignCheckpoint.Clear();
        foreach (var item in Catalog)
        {
            campaignCheckpoint[item.itemId] = GetCampaignItemCount(item.itemId);
        }
    }

    public static void RestoreCampaignCheckpoint()
    {
        if (campaignCheckpoint.Count == 0) return;
        foreach (var kvp in campaignCheckpoint)
        {
            PlayerPrefs.SetInt($"CampaignItem_{kvp.Key}", kvp.Value);
        }
        PlayerPrefs.Save();
    }

    public static List<MatchRewardEntry> GetMatchRewards(int matchNumber)
    {
        switch (matchNumber)
        {
            case 1:
                return new List<MatchRewardEntry>
                {
                    new MatchRewardEntry(POTION_ID, 1),
                    new MatchRewardEntry(PANACEA_ID, 2)
                };
            case 2:
                return new List<MatchRewardEntry>
                {
                    new MatchRewardEntry(GREAT_POTION_ID, 1),
                    new MatchRewardEntry(PANACEA_ID, 2)
                };
            case 3:
                return new List<MatchRewardEntry>
                {
                    new MatchRewardEntry(MEGA_POTION_ID, 1),
                    new MatchRewardEntry(ELIXIR_ID, 1)
                };
            case 4:
                return new List<MatchRewardEntry>
                {
                    new MatchRewardEntry(MEGA_POTION_ID, 2),
                    new MatchRewardEntry(ELIXIR_ID, 1),
                    new MatchRewardEntry(GREAT_ELIXIR_ID, 1)
                };
            case 5:
                return new List<MatchRewardEntry>
                {
                    new MatchRewardEntry(GREAT_ELIXIR_ID, 2),
                    new MatchRewardEntry(MEGA_POTION_ID, 3),
                    new MatchRewardEntry(MIRACLE_DEW_ID, 1)
                };
            default:
                return new List<MatchRewardEntry>();
        }
    }

    public static void GrantMatchRewards(int matchNumber)
    {
        var rewards = GetMatchRewards(matchNumber);
        foreach (var reward in rewards)
        {
            AddCampaignItem(reward.itemId, reward.amount);
        }
    }
}