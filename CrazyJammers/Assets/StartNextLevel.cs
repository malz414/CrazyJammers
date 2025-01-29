using UnityEngine;

public class StartNextLevel : MonoBehaviour
{
    public MainMenuManager MainMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainMenu.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
