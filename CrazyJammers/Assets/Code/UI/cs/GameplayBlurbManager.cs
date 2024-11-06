using Code.Core.Events;
using Code.Utility.Events;
using UnityEngine;

public class GameplayBlurbEvent : GenericEvent<string> { }

public class GameplayBlurbManager : MonoBehaviour
{

    [SerializeField] GameObject blurbPrefab;

    private void Awake()
    {
        EventBus.Subscribe<GameplayBlurbEvent>(OnGameplayBlurb);
    }

    private void OnGameplayBlurb(GameplayBlurbEvent blurbEvent)
    {
        string blurbText = blurbEvent.First;

        GameObject blurbObj = GameObject.Instantiate(blurbPrefab, gameObject.transform);

        GameplayBlurb blurb = blurbObj.GetComponent<GameplayBlurb>();

        blurb.Init(blurbText);

    }

}
