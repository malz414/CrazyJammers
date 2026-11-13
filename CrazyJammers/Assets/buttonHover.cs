using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class buttonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ButtonActionType 
    { 
        Normal, 
        Bide, 
        Potion, 
        Panacea, 
        SoulMeter,
        MiracleDew,
        BlessedAmulet
    }

    [Header("Button Settings")]
    [Tooltip("Select what this button does so it knows when to show 'Text No'.")]
    public ButtonActionType buttonType = ButtonActionType.Normal;
    
    [Tooltip("Check this to also send the description to the main move box (e.g., Soul Meter).")]
    public bool updateMainDescriptionBox = false;

    [Header("UI References")]
    public TMP_Text descText;
    
    [Header("Descriptions")]
    [TextArea(2,4)]
    public string text = "";
    [TextArea(2,4)]
    public string textNo = "";
    
    private Hero hero;

    void Start()
    {
        hero = FindObjectOfType<Hero>();
        if (hero == null)
        {
            Debug.LogError("Hero not found in the scene.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool isUnavailable = false;
        string customLockedMessage = "";
        
        bool itemActionLocked = EndlessModeManager.Instance != null && EndlessModeManager.Instance.hasUsedItemThisTurn;
        bool amuletActive = EndlessModeManager.Instance != null && EndlessModeManager.Instance.freeItemActionActive;

        switch (buttonType)
        {
            case ButtonActionType.Bide:
                if (hero != null && hero.bideLevel == 2.0f) isUnavailable = true;
                break;
            case ButtonActionType.SoulMeter:
                if (TurnManager.Instance != null && TurnManager.Instance.currentSoulShards < TurnManager.Instance.maxSoulShards) isUnavailable = true;
                break;
                
            case ButtonActionType.Potion:
                if (itemActionLocked) { isUnavailable = true; customLockedMessage = "You already used an item this turn!"; }
                else if (PotionData.Instance != null && PotionData.Instance.Potion <= 0) isUnavailable = true;
                break;
                
            case ButtonActionType.Panacea:
                if (itemActionLocked) { isUnavailable = true; customLockedMessage = "You already used an item this turn!"; }
                else if (PotionData.Instance != null && PotionData.Instance.Panacea <= 0) isUnavailable = true;
                break;
                
            case ButtonActionType.MiracleDew:
                if (itemActionLocked) { isUnavailable = true; customLockedMessage = "You already used an item this turn!"; }
                else if (EndlessModeManager.Instance == null || EndlessModeManager.Instance.GetSessionItemCount(EndlessShopData.MIRACLE_DEW_ID) <= 0) isUnavailable = true;
                break;
                
            case ButtonActionType.BlessedAmulet:
                if (itemActionLocked) { isUnavailable = true; customLockedMessage = "You already used an item this turn!"; }
                else if (amuletActive) { isUnavailable = true; customLockedMessage = "Blessed Amulet is already active!"; }
                else if (EndlessModeManager.Instance == null || EndlessModeManager.Instance.GetSessionItemCount(EndlessShopData.BLESSED_AMULET_ID) <= 0) isUnavailable = true;
                break;
        }

        // Display the correct text based on the lockout reason
        if (isUnavailable)
        {
            descText.text = !string.IsNullOrEmpty(customLockedMessage) ? customLockedMessage : textNo;
        }
        else
        {
            descText.text = text;
        }

        if (updateMainDescriptionBox && TurnManager.Instance != null)
        {
            TurnManager.Instance.ShowDescription(descText.text);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descText.text = "Select a command.";
        
        if (updateMainDescriptionBox && TurnManager.Instance != null)
        {
            TurnManager.Instance.HideDescription();
        }
    }
}