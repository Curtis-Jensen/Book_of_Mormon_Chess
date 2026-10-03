using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public List<MusicChoice> musicChoices;
    AudioSource source;

    /* If in official build of the game, remove the ability for copyrighted music to be played
     * it's just for fun while testing
     */
    void Start()
    {
        // RemoveAll instead of Remove inside foreach -- modifying the list mid-iteration throws
        if (!Application.isEditor)
            musicChoices.RemoveAll(song => song.isCopyrighted);
        if (musicChoices.Count == 0) return;

        source = gameObject.GetComponent<AudioSource>();
        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        source.clip = musicChoices[Random.Range(0, musicChoices.Count)].music;
        source.Play();
        yield return new WaitForSecondsRealtime(source.clip.length);
        source.Stop();
        StartCoroutine(PlayMusic());
    }
}

[System.Serializable]
public struct MusicChoice
{
    public AudioClip music;
    [Tooltip("Whether it should be deleted in builds or not")]
    public bool isCopyrighted;
}