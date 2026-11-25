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
    [SerializeField] float typeSpeed = 0.01f;
    public bool startInstantly;
    public bool FirstMatch;
    public bool mainMenu = false;
    public TransitionSettings sequenceTransition;

    void Awake()
    {
        EventBus.Subscribe<FadeOutEvent>(OnFadeOut);
        EventBus.Subscribe<FadeInEvent>(OnFadeIn);
        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }
    }

    void Start()
    {
        // First Match takes priority: type out the intro text and wait for the Next button
        if (FirstMatch && !mainMenu)
        {
            StartCoroutine(RunFirstMatchIntroSequence());
            return;
        }

        // Don't run battle setup logic if we are just in the Main Menu!
        if (startInstantly && !mainMenu)
        {
            if (skipped) return;
            skipped = true;

            if (fadeScreen != null)
            {
                fadeScreen.interactable = false;
                fadeScreen.blocksRaycasts = false;
                LeanTween.alphaCanvas(fadeScreen, 0, 0);
            }

            if (MusicManager.Instance != null)
                MusicManager.Instance.PlayMusic(MusicManager.Instance.prologueMusic2);

            if (quoteBoxManager != null && enemyQuote != null && quoteNum >= 0 && quoteNum < enemyQuote.Length)
                quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
        }
    }

    private IEnumerator RunFirstMatchIntroSequence()
    {
        skipped = false;

        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true);
            fadeScreen.alpha = 1f;
            fadeScreen.interactable = true;
            fadeScreen.blocksRaycasts = true;
        }

        if (skipButton != null) skipButton.SetActive(false);

        if (prologueTextUI != null && introText != null && introText.Length > 0)
        {
            yield return StartCoroutine(TypeText(introText[0]));
        }

        // Reveal the Next button after typing finishes; pressing it calls SkipToGame() to show the quote box
        if (skipButton != null) skipButton.SetActive(true);
    }

    // Wire your Next / Skip button's OnClick() event to this method
    public void SkipToGame()
    {
        if (skipped) return;

        skipped = true;
        StopAllCoroutines();

        if (skipButton != null) skipButton.SetActive(false);

        if (fadeScreen != null)
        {
            fadeScreen.interactable = false;
            fadeScreen.blocksRaycasts = false;
            LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(MusicManager.Instance.prologueMusic2);
        }

        if (!mainMenu && quoteBoxManager != null && enemyQuote != null && quoteNum >= 0 && quoteNum < enemyQuote.Length)
        {
            quoteBoxManager.SetQuoteBox(enemyQuote[quoteNum]);
        }
    }

    public void ShowIntro()
    {
        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator TypeText(string textToType)
    {
        prologueTextUI.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            prologueTextUI.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private void OnFadeOut(FadeOutEvent fadeEvent)
    {
        if (fadeEvent.category != this.category) { return; }
        else if (fadeEvent.category == UICategoryEnums.TransitionUI)
        {
            StartCoroutine(NextBattleTransitionCoroutine());
        }
    }

    private IEnumerator RunIntroSequence()
    {
        if (TransitionManager.Instance() != null && sequenceTransition != null)
            TransitionManager.Instance().Transition(sequenceTransition, 0f);

        yield return new WaitForSeconds(1f);

        if (skipButton != null) skipButton.SetActive(true);

        if (mainMenuScreen != null)
        {
            mainMenuScreen.interactable = false;
            mainMenuScreen.blocksRaycasts = false;
            mainMenuScreen.alpha = 0;
        }

        if (prologueTextUI != null && introText != null && introText.Length > 0)
        {
            yield return StartCoroutine(TypeText(introText[0]));
        }

        yield return new WaitForSeconds(5f);

        if (mainMenuManager != null)
        {
            mainMenuManager.HideCredits();
        }
    }

    private IEnumerator NextBattleTransitionCoroutine()
    {
        if (skipButton != null) skipButton.SetActive(true);
        if (prologueTextUI != null && introText != null && introText.Length > 0)
            prologueTextUI.text = introText[0];

        yield return new WaitForSeconds(30f);
        if (fadeScreen != null) LeanTween.alphaCanvas(fadeScreen, 0, FADE_OUT_TIME);
        SkipToGame();
    }

    private void OnFadeIn(FadeInEvent fadeEvent)
    {
        if (fadeEvent.category != this.category) { return; }
        if (fadeScreen != null) LeanTween.alphaCanvas(fadeScreen, 1, FADE_IN_TIME);
    }

    void OnDestroy()
    {
        EventBus.Unsubscribe<FadeOutEvent>(OnFadeOut);
        EventBus.Unsubscribe<FadeInEvent>(OnFadeIn);
    }
}