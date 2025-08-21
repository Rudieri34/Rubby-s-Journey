using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSoundManager : MonoBehaviour
{
    [SerializeField] private bool playingAmbienceSounds = true;
    [SerializeField] private Vector2 timeBetweenSounds;

    private void Start()
    {
        StartCoroutine(PlayAmbienceSound());
    }

    IEnumerator PlayAmbienceSound()
    { 
        while (playingAmbienceSounds)
        {
            yield return new WaitForSecondsRealtime(Random.Range(timeBetweenSounds.x, timeBetweenSounds.y));
            PlayRandomAmbienceSound();
        }
    }

    public void PlayRandomAmbienceSound()
    { 
        var instance = SoundManager.Instance;
        //instance.PlayAmbienceAudio(instance.ambienceAudioClips[Random.Range(0, instance.ambienceAudioClips.Count)], Random.Range(20,60));
    }
}
