using UnityEngine;

public class PlaySoundOnce : MonoBehaviour
{
    void Awake()
    {
        var audioSources = FindObjectsOfType<AudioSource>();

        // Count how many have the same clip (or same tag/component if needed)
        int sameClipCount = 0;
        foreach (var source in audioSources)
        {
            if (source.clip == GetComponent<AudioSource>().clip)
                sameClipCount++;
        }

        // If this clip already exists elsewhere, stop this one
        if (sameClipCount > 1)
        {
            GetComponent<AudioSource>().Stop();
        }
    }
}
