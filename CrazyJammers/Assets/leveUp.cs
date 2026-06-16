using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition; 


public class levelUp : MonoBehaviour
{
    public TransitionSettings sequenceTransition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(sequenceTransition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartNextLevel(string scene)
    {
       if (PotionData.Instance != null)
        {
            PotionData.Instance.Potion++;
            PotionData.Instance.Panacea++;
        }
                TransitionManager.Instance().Transition(scene, MusicManager.Instance.sequenceTransition, 0f);

    }
}
