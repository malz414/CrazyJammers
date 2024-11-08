using TMPro;
using UnityEngine;

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
        quoteBox.SetActive(true);
        quoteText.text = quote;
    }

    public void Next()
    {
        TurnManager.Instance.StartBattle();
        quoteBox.transform.parent.parent.gameObject.SetActive(false);
        quoteText.text = "";
    }
}
