using UnityEngine;
using EasyTransition; 


public class PotionData : MonoBehaviour
{
    public static PotionData Instance;

    public int Potion = 1;
    public int Panacea = 1;

    private int checkpointPotion;
    private int checkpointPanacea;
    public TransitionSettings sequenceTransition;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    public void SaveCheckpoint()
    {
        checkpointPotion = Potion;
        checkpointPanacea = Panacea;
    }

    public void RestoreCheckpoint()
    {
        Potion = checkpointPotion;
        Panacea = checkpointPanacea;
    }

    public void ResetValues()
    {
        Potion = 1;
        Panacea = 1;
    }
}