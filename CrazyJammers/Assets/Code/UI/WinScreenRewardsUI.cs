using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinScreenRewardsUI : MonoBehaviour
{
    [Header("Reward Slot 1")]
    [SerializeField] private GameObject reward1Group;
    [SerializeField] private TextMeshProUGUI uniqueItemLabelA;
    [SerializeField] private TextMeshProUGUI uniqueItemCountA;

    [Header("Reward Slot 2")]
    [SerializeField] private GameObject reward2Group;
    [SerializeField] private TextMeshProUGUI uniqueItemLabelB;
    [SerializeField] private TextMeshProUGUI uniqueItemCountB;

    [Header("Reward Slot 3 (Match 4 & 5 Only)")]
    [SerializeField] private GameObject reward3Group;
    [SerializeField] private TextMeshProUGUI uniqueItemLabelC;
    [SerializeField] private TextMeshProUGUI uniqueItemCountC;

    public void DisplayRewards(List<MatchRewardEntry> rewards)
    {
        SetSlot(reward1Group, uniqueItemLabelA, uniqueItemCountA, rewards, 0);
        SetSlot(reward2Group, uniqueItemLabelB, uniqueItemCountB, rewards, 1);
        SetSlot(reward3Group, uniqueItemLabelC, uniqueItemCountC, rewards, 2);
    }

    private void SetSlot(GameObject group, TextMeshProUGUI label, TextMeshProUGUI countText, List<MatchRewardEntry> rewards, int index)
    {
        if (group == null) return;

        if (index < rewards.Count)
        {
            var entry = rewards[index];
            var def = EndlessShopData.GetDefinition(entry.itemId);
            group.SetActive(true);

            if (label != null) label.text = def != null ? def.displayName : entry.itemId;
            if (countText != null) countText.text = $"x{entry.amount}";
        }
        else
        {
            group.SetActive(false);
        }
    }
}