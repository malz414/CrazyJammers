using UnityEngine;
using UnityEngine.SceneManagement;

public class levelUp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartNextLevel(string scene)
    {
          SceneManager.LoadScene(scene);
    }
}
