using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


public class buttonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text descText;
    public string text = "";
    public string textNo = "";
    private Hero hero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the TurnManager instance in the scene
        hero = FindObjectOfType<Hero>();
        if (hero == null)
        {
            Debug.LogError("TurnManager not found in the scene.");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.name == "Bide Button")
        { 
            Debug.Log("Bide Button hovered!");
            if(hero.bideLevel == 2.0f )
            {
                descText.text = textNo;
            }
            else
            {
                  descText.text = text;
            }
        }
        else
        {
            descText.text = text;
        }
         
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        descText.text = "Select a command.";
    }

        public void OnPointerClick(PointerEventData eventData)
    {
         if(PotionData.Instance.Potion > 0)
        {
            descText.text = textNo;
        }
       
    }
}
