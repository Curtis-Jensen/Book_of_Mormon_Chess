using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public List<MusicChoice> musicChoices;
    [Tooltip("Seconds of silence between one song ending and the next starting")]
    public float gapBetweenSongs = 3f;
    AudioSource source;
    int lastIndex = -1;

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
        while (true)
        {
            // Pick a random song, but never the same one twice in a row
            int next = Random.Range(0, musicChoices.Count);
            if (musicChoices.Count > 1 && next == lastIndex)
                next = (next + Random.Range(1, musicChoices.Count)) % musicChoices.Count;
            lastIndex = next;

            // Clips aren't preloaded, so make sure the data is ready before playing
            AudioClip clip = musicChoices[next].music;
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading)
                yield return null;

            source.clip = clip;
            source.loop = false;
            source.Play();

            // Wait for the song to actually finish instead of trusting clip.length
            yield return null;
            while (source.isPlaying || !Application.isFocused)
                yield return null;

            yield return new WaitForSecondsRealtime(gapBetweenSongs);
        }
    }
}

[System.Serializable]
public struct MusicChoice
{
    public AudioClip music;
    [Tooltip("Whether it should be deleted in builds or not")]
    public bool isCopyrighted;
}