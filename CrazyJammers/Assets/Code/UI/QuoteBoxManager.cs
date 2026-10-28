using TMPro;
using UnityEngine;
using System.Collections;
//using CrazyGames;

public class QuoteBoxManager : MonoBehaviour
{
    [SerializeField] GameObject quoteBox;
    [SerializeField] TextMeshProUGUI quoteText;
    public GameObject enemyHp = null;
    public GameObject HeroHp = null;
    public GameObject WaveUI = null;
    public GameObject soulMeterContainer = null; 
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
    // Safety check!
    if (TurnManager.Instance != null)
    {
        TurnManager.Instance.StartBattle();
    }
    
    yield return new WaitForSeconds(1.0f);
    quoteBox.SetActive(true);
    quoteText.text = quote;
}   

public void Next()
{
    if (TurnManager.Instance != null)
    {
        TurnManager.Instance.SetUpBattle();
    }
    
    MusicManager.Instance.PlayMusic(MusicManager.Instance.battleMusic);

    if(enemyHp != null) enemyHp.SetActive(true);
    if(HeroHp != null) HeroHp.SetActive(true);
    if(WaveUI != null) WaveUI.SetActive(true);
    if (soulMeterContainer != null) soulMeterContainer.SetActive(true);

    quoteBox.transform.parent.parent.gameObject.SetActive(false);
    quoteText.text = "";
}
}
