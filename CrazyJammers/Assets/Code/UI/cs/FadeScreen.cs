using Code.Core.Events;
using Code.Utility.Events;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutEvent : IBusEvent { }

public class FadeInEvent : IBusEvent { }

public class FadeScreen : MonoBehaviour
{

    private const float FADE_OUT_TIME = 1f;
    private const float FADE_IN_TIME = 1f;

    [SerializeField] CanvasGroup fadeScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        EventBus.Subscribe<FadeOutEvent>(OnFadeOut);
        EventBus.Subscribe<FadeInEvent>(OnFadeIn);

        //fadeScreen.alpha = 1;
    }

    private void OnFadeOut(FadeOutEvent fadeEvent)
    {
        LeanTween.alphaCanvas(fadeScreen, 1, FADE_OUT_TIME);
    }

    private void OnFadeIn(FadeInEvent fadeEvent)
    {
        LeanTween.alphaCanvas(fadeScreen, 0, FADE_IN_TIME);
    }
}
