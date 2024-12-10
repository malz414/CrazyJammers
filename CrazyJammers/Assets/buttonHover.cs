using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;


public class buttonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text descText;
    public string text = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        descText.text = text;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        descText.text = "Select a command.";
    }

        public void OnPointerClick(PointerEventData eventData)
    {
       descText.text = "Select a command.";
    }
}
