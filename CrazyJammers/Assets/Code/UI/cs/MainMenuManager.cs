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
        SceneManager.LoadScene("Scenes/CreditsScene");
    }
    public void HideCredits()
    {
        CreditsObj.SetActive(false);
    }

    private IEnumerator DoStartGameRoutine()
    {
        EventBus.Publish(new FadeOutEvent(UICategoryEnums.TransitionUI));

        yield return new WaitForSeconds(0f);

        gameObject.SetActive(false);
    }

}
