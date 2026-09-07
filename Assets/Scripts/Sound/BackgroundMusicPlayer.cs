using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BackgroundMusicPlayer : NetworkBehaviour
{
    public GameManager gameManager;

    public AudioSource audioSource;
    private float volume;

    public AudioClip act1Music;
    public AudioClip act2Music;
    public AudioClip act3Music;
    public AudioClip act4Music;

    private Coroutine musicTransitionCoroutine = null;

    public override void OnNetworkSpawn()
    {
        gameManager.currentAct.OnValueChanged += OnActChanged;
        volume = audioSource.volume;
    }

    public override void OnNetworkDespawn()
    {
        gameManager.currentAct.OnValueChanged -= OnActChanged;
    }

    private void OnActChanged(int previous, int current)
    {
        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
            audioSource.volume = volume;
        }

        musicTransitionCoroutine = StartCoroutine(MusicTransitionCoroutine(current));
    }

    private IEnumerator MusicTransitionCoroutine(int index)
    {
        Debug.Log($"Transitioning to music for act {index}");

        yield return StartCoroutine(AudioFader.FadeOut(audioSource, 3f));
        audioSource.clip = GetClipBySceneIndex(index);
        yield return StartCoroutine(AudioFader.FadeIn(audioSource, 3f));
    }

    private AudioClip GetClipBySceneIndex(int index)
    {
        AudioClip clip;

        switch (index)
        {
            case 1:
                clip = act1Music;
                break;
            case 2:
                clip = act2Music;
                break;
            case 3:
                clip = act3Music;
                break;
            case 4:
                clip = act4Music;
                break;
            default:
                clip = null;
                break;
        }

        return clip;
    }
}
