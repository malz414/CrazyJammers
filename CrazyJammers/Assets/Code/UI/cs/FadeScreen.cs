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
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip PrologueMusic;
    [SerializeField] private AudioClip BattleMusic;


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

    public void SkipToGame()
    {
        StopAllCoroutines();
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        quoteBoxManager.SetQuoteBox(enemyQuote[0]);
        audioSrc.clip = PrologueMusic;
        audioSrc.Play();
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
        yield return new WaitForSeconds(4f);
        audioSrc.clip = BattleMusic;
        audioSrc.Play();
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        quoteBoxManager.SetQuoteBox(enemyQuote[1]);
    }

    private void OnFadeIn(FadeInEvent fadeEvent)
    {
        if (fadeEvent.category != this.category) { return; }
        LeanTween.alphaCanvas(fadeScreen, 1, FADE_IN_TIME);
    }
}
