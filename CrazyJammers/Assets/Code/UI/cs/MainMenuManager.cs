using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using EasyTransition; 
#if USE_CRAZYGAMES
using CrazyGames;
#endif
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
    
    [Header("Continue Settings")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }
#endif

        bool isEndlessUnlocked = PlayerPrefs.GetInt("EndlessModeUnlocked", 0) == 1;

        // Completely hide the button if not unlocked
        if (endlessModeButton != null)
        {
            endlessModeButton.gameObject.SetActive(isEndlessUnlocked);
        }
        
        if (continueButton != null)
        {
            string savedScene = PlayerPrefs.GetString("SavedLevel", "");
            
            bool canContinue = !string.IsNullOrEmpty(savedScene) && savedScene != "GameScene(LVL1)";
            
            continueButton.interactable = canContinue;
            TextMeshProUGUI continueText = continueButton.GetComponentInChildren<TextMeshProUGUI>();

            if (continueText != null)
            {
                Color c = continueText.color;
                c.a = canContinue ? 1f : 0.4f; 
                continueText.color = c;
            }
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

    public void ContinueGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        string sceneToLoad = PlayerPrefs.GetString("SavedLevel", "GameScene(LVL1)");
        TransitionManager.Instance().Transition(sceneToLoad, sequenceTransition, 0f);
    }

    public void ShowCredits()
    {
#if USE_CRAZYGAMES
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Game.GameplayStop();
        }
#endif

        MusicManager.Instance.PlayMusic(MusicManager.Instance.credits);

        TransitionManager.Instance().Transition("CreditsScene", sequenceTransition, 0f);
    }

    public void HideCredits()
    {
        PlayerPrefs.DeleteKey("SavedLevel");
        PlayerPrefs.Save();

        TransitionManager.Instance().Transition("GameScene(LVL1)", sequenceTransition, 0f);
    }

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