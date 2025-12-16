using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Setup")]
    [Tooltip("Drag your Audio Source here")]
    public AudioSource audioSource;
    
    [Tooltip("Drag your 8 (or more) songs here")]
    public AudioClip[] songs;

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float maxVolume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (audioSource != null) audioSource.Stop(); 
            
            gameObject.SetActive(false); 
            
            Destroy(gameObject); 
        }
    }

    private void Start()
    {
        if(audioSource != null)
        {
            audioSource.volume = maxVolume;
        }
    }

    
    public void ChangeSong(int songIndex)
    {
        if (songIndex < 0 || songIndex >= songs.Length)
        {
            Debug.LogError($"MusicManager: Song index {songIndex} is out of range!");
            return;
        }

        if (audioSource.clip == songs[songIndex] && audioSource.isPlaying)
            return;

        StartCoroutine(FadeToNextSong(songs[songIndex]));
    }

    private IEnumerator FadeToNextSong(AudioClip nextClip)
    {
        float startVolume = audioSource.volume;

        if (audioSource.isPlaying)
        {
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null; 
            }
            audioSource.volume = 0;
            audioSource.Stop();
        }

        audioSource.clip = nextClip;
        audioSource.Play();
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = maxVolume;
    }
}