using UnityEngine;
using System.Collections;
using EasyTransition; 


public class train : MonoBehaviour
{

    

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float maxVolume = 1.0f;
    public TransitionSettings sequenceTransition;
    
    public void HideCreditsReal()
    {
       // MusicManager.Instance.PlayMusic(MusicManager.Instance.midnightMasquerade);

        TransitionManager.Instance().Transition("MainMenu", MusicManager.Instance.sequenceTransition, 0f);

    }


}