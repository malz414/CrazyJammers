using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DropdownManager : MonoBehaviour
{
    public TMP_Dropdown dropdown1;
    public TMP_Dropdown dropdown2;
    public TurnManager turnManager;
    private Transform dropdownListParent;
    private List<AttackSO> attacks;

    private void Update()
    {
        if (dropdown1.IsExpanded)
        {
            HandleDropdownOpened(dropdown1);
        }
        else if (dropdown2.IsExpanded)
        {
            HandleDropdownOpened(dropdown2);
        }
        else
        {
            if (dropdownListParent != null)
            {
                dropdownListParent = null;
            }
        }
    }

    private void HandleDropdownOpened(TMP_Dropdown dropdown)
    {
        if (dropdownListParent == null)
        {
            dropdownListParent = dropdown.transform.Find("Dropdown List/Viewport/Content");
            if (dropdownListParent == null)
            {
                return;
            }
        }

        FetchAttacksFromTurnManager();
        UpdateDropdownList();
    }

    private void FetchAttacksFromTurnManager()
    {
        if (turnManager == null)
        {
            return;
        }

        attacks = new List<AttackSO>(turnManager.enemyAttacksByIndex);
    }

    private void UpdateDropdownList()
    {
        if (dropdownListParent == null || attacks == null || attacks.Count == 0)
        {
            return;
        }

        for (int i = 1; i < dropdownListParent.childCount; i++) 
        {
            Transform item = dropdownListParent.GetChild(i);
            TMP_Text itemLabel = item.GetComponentInChildren<TMP_Text>();

            if (itemLabel != null && i - 1 < attacks.Count && attacks[i - 1] != null)
            {
                EnableBideLevels(item, attacks[i - 1]);
            }
        }
    }

    private void EnableBideLevels(Transform item, AttackSO attack)
    {
        for (int i = 2; i < attack.upgradeLevel + 2; i++)
        {
            if (i < item.childCount)
            {
                Transform child = item.GetChild(i);
                child.gameObject.SetActive(true);

                if (child.name.Contains("Bide Level"))
                {
                    TMP_Text childText = child.GetComponentInChildren<TMP_Text>();
                    if (childText != null)
                    {
                        string levelText = $"Level {i - 1}";
                        childText.text = levelText;
                    }
                }
            }
        }
    }
}
