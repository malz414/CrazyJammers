using Code.Utility.Events;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    private bool gameStarted = false;

    [SerializeField] private GameObject CreditsObj;

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
        SceneManager.LoadScene("CreditsScene");
    }
    public void HideCredits()
    {
         SceneManager.LoadScene("GameScene");
    }

    private IEnumerator DoStartGameRoutine()
    {
        EventBus.Publish(new FadeOutEvent(UICategoryEnums.TransitionUI));

        yield return new WaitForSeconds(0f);

        gameObject.SetActive(false);
    }
    public void ResetLevel()
    {
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
