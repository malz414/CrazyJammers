using UnityEngine;
using System.Collections;
using EasyTransition; 
using UnityEngine.SceneManagement;

#if USE_CRAZYGAMES
using CrazyGames;
#endif

public class train : MonoBehaviour
{
    public static train Instance;

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float maxVolume = 1.0f;
    public TransitionSettings sequenceTransition;

    public void HideCreditsReal()
    {
#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }
#endif

        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }
    
    public void ShowCredits()
    {
#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }
#endif

        TransitionManager.Instance().Transition("CreditsScene", sequenceTransition, 0f);
    }
}