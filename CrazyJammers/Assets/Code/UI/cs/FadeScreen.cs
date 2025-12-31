using Code.Core.Events;
using Code.Utility.Events;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class FadeOutEvent : IBusEvent
{
    public UICategoryEnums category { get; private set; }

    public FadeOutEvent(UICategoryEnums category)
    {
        this.category = category;
    }
}
public class FadeInEvent : IBusEvent 
{
    public UICategoryEnums category { get; private set; }
    public FadeInEvent(UICategoryEnums category)
    {
        this.category = category;
    }
}

public class FadeScreen : MonoBehaviour
{
    private bool skipped;
    private const float FADE_OUT_TIME = 1f;
    private const float FADE_IN_TIME = 1f;

    [TextArea(3, 10)]
    [SerializeField] string[] introText;
    [TextArea(3, 10)]
    [SerializeField] string[] enemyQuote;
    [SerializeField] UICategoryEnums category;
    [SerializeField] CanvasGroup fadeScreen;
    [SerializeField] TextMeshProUGUI prologueTextUI;
    [SerializeField] GameObject skipButton;
    [SerializeField] QuoteBoxManager quoteBoxManager;
    [SerializeField] int quoteNum;
    public bool startInstantly;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        EventBus.Subscribe<FadeOutEvent>(OnFadeOut);
        EventBus.Subscribe<FadeInEvent>(OnFadeIn);
        if(skipButton != null)
        {
            skipButton.SetActive(false);
        }
        
    }
    void Start()
    {
        if(startInstantly)
        {

            if (skipped) return;
            skipped=true;
            LeanTween.alphaCanvas(fadeScreen, 0, 0);
            MusicManager.Instance.ChangeSong(4);
            quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
            
            

        }
        
    }

    public void SkipToGame()
    {
        if (skipped) return;
        skipped=true;
        StopAllCoroutines();
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        MusicManager.Instance.ChangeSong(4);
        quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
    }

    private void OnFadeOut(FadeOutEvent fadeEvent)
    {
        if(fadeEvent.category != this.category) { return; }       
        else if (fadeEvent.category == UICategoryEnums.TransitionUI)
        {
            StartCoroutine(NextBattleTransitionCoroutine());
        }
    }


    private IEnumerator NextBattleTransitionCoroutine()
    {
        skipButton.SetActive(true);
        prologueTextUI.text = introText[0];
        yield return new WaitForSeconds(30f);
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        SkipToGame();
    }

    private void OnFadeIn(FadeInEvent fadeEvent)
    {
        if (fadeEvent.category != this.category) { return; }
        LeanTween.alphaCanvas(fadeScreen, 1, FADE_IN_TIME);
    }
    void OnDestroy()
    {
        EventBus.Unsubscribe<FadeOutEvent>(OnFadeOut);
        EventBus.Unsubscribe<FadeInEvent>(OnFadeIn);
    }
}
