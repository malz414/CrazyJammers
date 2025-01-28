using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class DropdownManager : MonoBehaviour
{
    public TMP_Dropdown dropdown1;
    public TMP_Dropdown dropdown2;
    public TurnManager turnManager;
    public TextMeshProUGUI descriptionText; 
    private Transform dropdownListParent;
    private List<AttackSO> attacks;

    private bool previousDropdown1State = false; 
    private bool previousDropdown2State = false; 

    private void Update()
    {
        // Check if dropdown1 was just opened
        if (dropdown1.IsExpanded && !previousDropdown1State)
        {
            HandleDropdownOpened(dropdown1);
            turnManager.drop1Opened = true;
        }
        else if (!dropdown1.IsExpanded && previousDropdown1State)
        {
            turnManager.drop1Opened = false;
        }

        // Check if dropdown2 was just opened
        if (dropdown2.IsExpanded && !previousDropdown2State)
        {
            HandleDropdownOpened(dropdown2);
            turnManager.drop2Opened = true;
        }
        else if (!dropdown2.IsExpanded && previousDropdown2State)
        {
            turnManager.drop2Opened = false;
        }

        // Update the tracked states
        previousDropdown1State = dropdown1.IsExpanded;
        previousDropdown2State = dropdown2.IsExpanded;
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
                AttachHoverListener(item, attacks[i - 1].attackDescription);
            }
        }
    }

    private void EnableBideLevels(Transform item, AttackSO attack)
    {
        if (turnManager.bideAttribute >= 1)
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
        else if (turnManager.bideAttribute < 1)
        {
            for (int i = 2; i < 3 + 2; i++)
            {
                if (i < item.childCount)
                {
                    Transform child = item.GetChild(i);
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    private void AttachHoverListener(Transform item, string description)
    {
        EventTrigger eventTrigger = item.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = item.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnterEntry.callback.AddListener((eventData) => ShowDescription(description));
        eventTrigger.triggers.Add(pointerEnterEntry);

        
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExitEntry.callback.AddListener((eventData) => ShowDescription(""));
        eventTrigger.triggers.Add(pointerExitEntry);
    }

    private void ShowDescription(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
            Debug.Log("DESCRIPTION: " + description);
        }
        else
        {
            Debug.Log("NO DESCRIPTION TEXT ASSIGNED");
        }
    }
}
