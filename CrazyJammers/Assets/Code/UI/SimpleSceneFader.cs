using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleSceneFader : MonoBehaviour
{
    public static SimpleSceneFader Instance;

    [Header("Setup")]
    [Tooltip("Drag the CanvasGroup of your black fade screen here")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the fader alive between scenes!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this to load a scene by name
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName, false, 0));
    }

    // Call this to load a scene by build index (great for restarting levels)
    public void FadeToScene(int sceneIndex)
    {
        StartCoroutine(TransitionRoutine("", true, sceneIndex));
    }

    private IEnumerator TransitionRoutine(string sceneName, bool useIndex, int sceneIndex)
    {
        // 1. Turn on blockRaycasts so the player can't click buttons while fading
        fadeCanvasGroup.blocksRaycasts = true;

        // 2. Fade screen to black
        LeanTween.alphaCanvas(fadeCanvasGroup, 1f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        // 3. Load the new Scene
        if (useIndex)
            SceneManager.LoadScene(sceneIndex);
        else
            SceneManager.LoadScene(sceneName);

        // 4. Fade screen back to clear
        LeanTween.alphaCanvas(fadeCanvasGroup, 0f, fadeTime);
        
        // 5. Allow clicking again
        fadeCanvasGroup.blocksRaycasts = false;
    }
}