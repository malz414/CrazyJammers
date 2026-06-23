using Code.Core.Events;
using Code.Utility.Events;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EasyTransition; 

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
    [SerializeField] CanvasGroup mainMenuScreen;
    [SerializeField] TextMeshProUGUI prologueTextUI;
    [SerializeField] GameObject skipButton;
    [SerializeField] QuoteBoxManager quoteBoxManager;
    [SerializeField] MainMenuManager mainMenuManager;
    [SerializeField] int quoteNum;
    public bool startInstantly;
    public bool FirstMatch;
    public bool mainMenu = false;
    public TransitionSettings sequenceTransition;




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
            MusicManager.Instance.PlayMusic(MusicManager.Instance.prologueMusic2);
            
            quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
            
            
            
            

        }
        if(FirstMatch)
        {

            StartCoroutine(RunIntroSequence()); //
        }

        
        
    }

    public void SkipToGame()
    {
        if (skipped) return;

        skipped=true;
        StopAllCoroutines();
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        MusicManager.Instance.PlayMusic(MusicManager.Instance.prologueMusic2);
        if(!mainMenu)
        {
        quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
        }
    }
    public void ShowIntro()
    {
        StartCoroutine(RunIntroSequence()); //
    }

    private IEnumerator TypeText(string textToType)
{
    prologueTextUI.text = ""; // Clear current text
    foreach (char letter in textToType.ToCharArray())
    {
        prologueTextUI.text += letter;
        // Adjust this wait time for faster or slower typing
        yield return new WaitForSeconds(0.01f); 
    }
}

    private void OnFadeOut(FadeOutEvent fadeEvent)
    {
        if(fadeEvent.category != this.category) { return; }       
        else if (fadeEvent.category == UICategoryEnums.TransitionUI)
        {
            StartCoroutine(NextBattleTransitionCoroutine());
        }
    }
    private IEnumerator RunIntroSequence()
{       TransitionManager.Instance().Transition(sequenceTransition, 0f);
        yield return new WaitForSeconds(1f); 
        skipButton.SetActive(true);
        mainMenuScreen.interactable = false;
        mainMenuScreen.blocksRaycasts = false;
        
        // Instantly hide the main menu (since the transition handles the visual flair now)
        mainMenuScreen.alpha = 0; 
        
        yield return StartCoroutine(TypeText(introText[0]));
        
        yield return new WaitForSeconds(5f); 
        
        // Trigger the in-scene transition to reveal the next state
        // 0f is the start delay before the transition begins
        
        mainMenuManager.HideCredits();
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
