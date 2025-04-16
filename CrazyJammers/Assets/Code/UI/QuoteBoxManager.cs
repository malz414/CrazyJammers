using TMPro;
using UnityEngine;
using System.Collections;
using CrazyGames;

public class QuoteBoxManager : MonoBehaviour
{
    [SerializeField] GameObject quoteBox;
    [SerializeField] TextMeshProUGUI quoteText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        quoteBox.SetActive(false);
    }


    public void SetQuoteBox(string quote)
    {
        StartCoroutine(setQuote(quote));

     
    }

    private IEnumerator setQuote(string quote)
    {
        TurnManager.Instance.StartBattle();
        yield return new WaitForSeconds(1.0f);
        quoteBox.SetActive(true);
        quoteText.text = quote;
    }   

    public void Next()
    {
        TurnManager.Instance.SetUpBattle();
        
        quoteBox.transform.parent.parent.gameObject.SetActive(false);
        quoteText.text = "";
        CrazySDK.Game.GameplayStart();
    }
}
