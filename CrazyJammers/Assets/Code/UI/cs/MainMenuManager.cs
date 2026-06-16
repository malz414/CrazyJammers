using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition; 


public class MainMenuManager : MonoBehaviour
{

    private bool gameStarted = false;

    [SerializeField] private GameObject CreditsObj;
        public TransitionSettings sequenceTransition;


    public void StartGame()
    {
        if(gameStarted)
        {
            return;
        }
        
        gameStarted = true;

        StartCoroutine(DoStartGameRoutine());
    }

    public void ShowCredits()
    {
        MusicManager.Instance.PlayMusic(MusicManager.Instance.credits);

        TransitionManager.Instance().Transition("CreditsScene", sequenceTransition, 0f);
    }
    public void HideCredits()
    {

        TransitionManager.Instance().Transition("GameScene(LVL1)", sequenceTransition, 0f);

    }
     public void HideCreditsReal()
    {
        MusicManager.Instance.PlayMusic(MusicManager.Instance.midnightMasquerade);

        SceneManager.LoadScene("MainMenu");

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
