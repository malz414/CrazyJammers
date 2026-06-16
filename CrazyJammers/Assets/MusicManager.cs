using UnityEngine;
using System.Collections;
using EasyTransition; 


public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Setup")]
    [Tooltip("Drag your Audio Source here")]
    public AudioSource audioSource;
    
    [Header("Songs")]
    public AudioClip battleMusic;
    public AudioClip preMatchMusic;
    public AudioClip prologueMusic;
    public AudioClip prologueMusicHalf; // Prologue Music.5
    public AudioClip prologueMusic2;
    public AudioClip midnightMasquerade;
    public AudioClip jingle12;
    public AudioClip gameOver;
    
    public AudioClip credits;
    

    [Header("Settings")]
    public float fadeDuration = 1.0f; 
    public float maxVolume = 1.0f;
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

    public void PlayMusic(AudioClip nextClip)
    {
        if (nextClip == null)
        {
            Debug.LogWarning("MusicManager: Tried to play a null audio clip!");
            return;
        }

        if (audioSource.clip == nextClip && audioSource.isPlaying)
            return;

        StartCoroutine(FadeToNextSong(nextClip, true)); // true = looping
    }
    public void HideCreditsReal()
    {
        MusicManager.Instance.PlayMusic(MusicManager.Instance.midnightMasquerade);

        TransitionManager.Instance().Transition("MainMenu", sequenceTransition, 0f);

    }

    public void PlayMusicOnce(AudioClip nextClip)
    {
        if (nextClip == null)
        {
            Debug.LogWarning("MusicManager: Tried to play a null audio clip!");
            return;
        }

        if (audioSource.clip == nextClip && audioSource.isPlaying)
            return;

        StartCoroutine(FadeToNextSong(nextClip, false)); // false = no looping
    }

    private IEnumerator FadeToNextSong(AudioClip nextClip, bool shouldLoop)
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
        
        audioSource.loop = shouldLoop;

        nextClip.LoadAudioData(); 
        while (nextClip.loadState != AudioDataLoadState.Loaded)
        {
            yield return null; 
        }
        audioSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = maxVolume;
    }
}