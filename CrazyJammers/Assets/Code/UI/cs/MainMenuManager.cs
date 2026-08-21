using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using EasyTransition; 
using CrazyGames;
using TMPro; 

public class MainMenuManager : MonoBehaviour
{

    private bool gameStarted = false;

    [SerializeField] private GameObject CreditsObj;
        public TransitionSettings sequenceTransition;
        public TransitionSettings rectangle;
        [Header("Mode Unlocks")]
    [SerializeField] private Button endlessModeButton; 
    [SerializeField] private string endlessSceneName = "EndlessModeScene"; 
    public bool MainMenu;


    private void Start()
    {
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }

        bool isEndlessUnlocked = PlayerPrefs.GetInt("EndlessModeUnlocked", 0) == 1;

        if (endlessModeButton != null)
        {
            endlessModeButton.interactable = isEndlessUnlocked;

            TextMeshProUGUI buttonText = endlessModeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                
                textColor.a = isEndlessUnlocked ? 1f : 0.4f; 
                
                buttonText.color = textColor;
            }
        }
        
        if(MainMenu && MusicManager.Instance != null)
        {
           // MusicManager.Instance.PlayMusic(MusicManager.Instance.midnightMasquerade);
        }

    }
    public void StartGame()
    {
        if(gameStarted)
        {
            return;
        }
        
        gameStarted = true;

        StartCoroutine(DoStartGameRoutine());
    }
    public void StartEndlessMode()
    {
        if (gameStarted) return;
        gameStarted = true;

        TransitionManager.Instance().Transition(endlessSceneName, sequenceTransition, 0f);
    }

    public void ShowCredits()
    {
         if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }

       // MusicManager.Instance.PlayMusic(MusicManager.Instance.credits);

        TransitionManager.Instance().Transition("CreditsScene", sequenceTransition, 0f);
    }
    public void HideCredits()
    {

        TransitionManager.Instance().Transition("GameScene(LVL5)", sequenceTransition, 0f);

    }
     public void HideCreditsReal()
    {
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }


        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }


    private IEnumerator DoStartGameRoutine()
    {
        EventBus.Publish(new FadeOutEvent(UICategoryEnums.TransitionUI));

        yield return new WaitForSeconds(0f);

        gameObject.SetActive(false);
    }
    public void ResetLevel()
    {
        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }

}
