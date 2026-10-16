using UnityEngine;
using System.Collections;
using EasyTransition; 
using UnityEngine.SceneManagement;
using CrazyGames;




public class train : MonoBehaviour
{
    public static train Instance;

    

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float maxVolume = 1.0f;
    public TransitionSettings sequenceTransition;



    
    public void HideCreditsReal()
    {
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }

        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }
      public void ShowCredits()
    {
          if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }

        TransitionManager.Instance().Transition("CreditsScene", sequenceTransition, 0f);
    }


}