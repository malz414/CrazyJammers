using UnityEngine;

public class PotionData : MonoBehaviour
{
    public static PotionData Instance;

    public int Potion = 1;
    public int Panacea = 1;

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

    public void ResetValues()
    {
        Debug.Log("Values Reset");
        Potion = 1;
        Panacea = 1;
    }
}
