using Code.Core.Events;
using Code.Utility.Events;
using UnityEngine;


public enum SoundEffect
{
    Click
}
public enum MusicTheme
{
    MainTheme
}



public class PlaySFXEvent : GenericEvent<SoundEffect> { }
public class PlayMusicEvent : GenericEvent<MusicTheme> { }

public class AudioManager : MonoBehaviour
{

    [SerializeField] AudioSource sfxSource;

    [SerializeField] AudioSource musicSource;

    //Music

    [SerializeField] AudioClip mainThemeAudio;


    //Sound effects

    [SerializeField] AudioClip clickAudio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.Subscribe<PlaySFXEvent>(OnPlaySFX);
        EventBus.Subscribe<PlayMusicEvent>(OnMusicTheme);
    }

    void OnPlaySFX(PlaySFXEvent sfxEvent)
    {
        AudioClip clip = GetAudioClipForSoundEffect(sfxEvent.First);

        sfxSource.PlayOneShot(clip);

    }

    private AudioClip GetAudioClipForSoundEffect(SoundEffect effect)
    {
        switch(effect)
        {
            case SoundEffect.Click:
                return clickAudio;
        }


        Debug.LogError($"Unhandled sound effect: {effect}");
        return null;
    }

    private void OnMusicTheme(PlayMusicEvent musicEvent)
    {
        AudioClip clip = GetAudioClipForMusicTheme(musicEvent.First);

        musicSource.clip = clip;
        musicSource.Play();
    }

    private AudioClip GetAudioClipForMusicTheme(MusicTheme theme)
    {
        switch (theme)
        {
            case MusicTheme.MainTheme:
                return mainThemeAudio;
        }


        Debug.LogError($"Unhandled music: {theme}");
        return null;
    }


}
