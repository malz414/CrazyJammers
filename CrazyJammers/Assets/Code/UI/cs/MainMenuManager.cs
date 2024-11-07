using Code.Utility.Events;
using System.Collections;
using UnityEngine;

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
        CreditsObj.SetActive(true);
    }
    public void HideCredits()
    {
        CreditsObj.SetActive(false);
    }

    private IEnumerator DoStartGameRoutine()
    {
        EventBus.Publish(new FadeOutEvent(UICategoryEnums.OpeningTransitionUI));

        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
    }

}
