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

    [Header("Prologue / New Game")]
    [Tooltip("If your Prologue is a separate scene, enter its exact scene name here (e.g. 'PrologueScene').")]
    [SerializeField] private string prologueSceneName = "PrologueScene";
    [Tooltip("Optional: If your Prologue is a GameObject inside the MainMenu scene, drag it here.")]
    [SerializeField] private GameObject prologueObject;

    [Header("Menus & Transitions")]
    [SerializeField] private GameObject CreditsObj;
    public TransitionSettings sequenceTransition;
    public TransitionSettings rectangle;

    [Header("Mode Unlocks & Notifications")]
    [SerializeField] private Button endlessModeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private GameObject endlessUnlockNotifOverlay;
    [SerializeField] private GameObject shopUnlockNotifOverlay;
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

        bool isEndlessUnlocked = EndlessShopData.IsEndlessUnlocked;
        if (endlessModeButton != null) endlessModeButton.gameObject.SetActive(isEndlessUnlocked);
        if (shopButton != null) shopButton.gameObject.SetActive(EndlessShopData.IsShopUnlocked);
        Debug.Log($"[MainMenuNotifDebug] Scene='{SceneManager.GetActiveScene().name}' | " +
                  $"EndlessUnlocked={isEndlessUnlocked} | EndlessShown={EndlessShopData.HasShownEndlessUnlockNotif} | " +
                  $"EndlessOverlay={(endlessUnlockNotifOverlay != null ? endlessUnlockNotifOverlay.name : "NULL")} | " +
                  $"IsSceneObj={(endlessUnlockNotifOverlay != null && endlessUnlockNotifOverlay.scene.IsValid())} | " +
                  $"ShopUnlocked={EndlessShopData.IsShopUnlocked} | ShopShown={EndlessShopData.HasShownShopUnlockNotif}");

        // Only trigger unlock popups on the actual MainMenu scene (never on LastScreen)
        if (string.Equals(SceneManager.GetActiveScene().name, "MainMenu", System.StringComparison.OrdinalIgnoreCase))
        {
            if (EndlessShopData.IsShopUnlocked && !EndlessShopData.HasShownShopUnlockNotif && shopUnlockNotifOverlay != null)
            {
                if (endlessUnlockNotifOverlay != null) endlessUnlockNotifOverlay.SetActive(false);
                ActivatePopupWithParents(shopUnlockNotifOverlay);
                EndlessShopData.HasShownShopUnlockNotif = true;
            }
            else if (isEndlessUnlocked && !EndlessShopData.HasShownEndlessUnlockNotif && endlessUnlockNotifOverlay != null)
            {
                if (shopUnlockNotifOverlay != null) shopUnlockNotifOverlay.SetActive(false);
                ActivatePopupWithParents(endlessUnlockNotifOverlay);
                Debug.Log($"[MainMenuNotifDebug] Activated Endless Popup -> activeSelf={endlessUnlockNotifOverlay.activeSelf} | activeInHierarchy={endlessUnlockNotifOverlay.activeInHierarchy}");
                EndlessShopData.HasShownEndlessUnlockNotif = true;
            }
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

    private void ActivatePopupWithParents(GameObject popup)
    {
        Transform p = popup.transform.parent;
        while (p != null && p.GetComponent<Canvas>() == null)
        {
            p.gameObject.SetActive(true);
            p = p.parent;
        }
        popup.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Debug] All PlayerPrefs Wiped! Restarting scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        StartCoroutine(DoStartGameRoutine());
    }

    public void StartEndlessMode()
    {
        if (gameStarted) return;
        gameStarted = true;

        EndlessShopData.IsEndlessUnlocked = true;
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
        if (CrazySDK.IsAvailable) CrazySDK.Game.GameplayStop();
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
        if (CrazySDK.IsAvailable) CrazySDK.Game.GameplayStop();
#endif
        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }

    private IEnumerator DoStartGameRoutine()
    {
        EventBus.Publish(new FadeOutEvent(UICategoryEnums.TransitionUI));

        if (prologueObject != null)
        {
            prologueObject.SetActive(true);
            gameObject.SetActive(false);
            yield break;
        }

        if (!string.IsNullOrEmpty(prologueSceneName) && Application.CanStreamedLevelBeLoaded(prologueSceneName))
        {
            if (TransitionManager.Instance() != null && sequenceTransition != null)
                TransitionManager.Instance().Transition(prologueSceneName, sequenceTransition, 0f);
            else
                SceneManager.LoadScene(prologueSceneName);
            yield break;
        }

        yield return new WaitForSeconds(0f);
        gameObject.SetActive(false);
    }

    public void ResetLevel()
    {
        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);
    }

    public void GoToShop()
    {
        TransitionManager.Instance().Transition("ShopScene", sequenceTransition, 0f);
    }

    public void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}