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
        SoulMeter 
    }

    [Header("Button Settings")]
    [Tooltip("Select what this button does so it knows when to show 'Text No'.")]
    public ButtonActionType buttonType = ButtonActionType.Normal;
    
    [Tooltip("Check this to also send the description to the main move box (e.g., Soul Meter).")]
    public bool updateMainDescriptionBox = false;

    [Header("UI References")]
    public TMP_Text descText;
    public TMP_Text potText2;
    
    [Header("Descriptions")]
    [TextArea(2,4)]
    public string text = "";
    [TextArea(2,4)]
    public string textNo = "";
    
    private Hero hero;

    void Start()
    {
        // Find the Hero instance in the scene
        hero = FindObjectOfType<Hero>();
        if (hero == null)
        {
            Debug.LogError("Hero not found in the scene.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool isUnavailable = false;

        // Check availability based on the button type chosen in the Inspector
        switch (buttonType)
        {
            case ButtonActionType.Bide:
                if (hero != null && hero.bideLevel == 2.0f) isUnavailable = true;
                break;
            case ButtonActionType.Potion:
                if (PotionData.Instance != null && PotionData.Instance.Potion <= 0) isUnavailable = true;
                break;
            case ButtonActionType.Panacea:
                if (PotionData.Instance != null && PotionData.Instance.Panacea <= 0) isUnavailable = true;
                break;
            case ButtonActionType.SoulMeter:
                if (TurnManager.Instance != null && TurnManager.Instance.currentSoulShards < TurnManager.Instance.maxSoulShards) isUnavailable = true;
                break;
        }

        // Apply the correct text
        if (isUnavailable)
        {
            descText.text = textNo;
            if(potText2 != null) potText2.text = textNo;

        }
        else
        {
            descText.text = text;
            if(potText2 != null) potText2.text = text;
        }

        // Push the same description to the main move box if checked
        if (updateMainDescriptionBox && TurnManager.Instance != null)
        {
            TurnManager.Instance.ShowDescription(descText.text);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descText.text = "Select a command.";
        if(potText2 != null) potText2.text = "Select a command.";

        
        // Reset the main move box if checked
        if (updateMainDescriptionBox && TurnManager.Instance != null)
        {
            TurnManager.Instance.HideDescription();
        }
    }
}