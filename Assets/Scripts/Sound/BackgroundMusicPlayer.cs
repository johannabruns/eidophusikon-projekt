using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BackgroundMusicPlayer : NetworkBehaviour
{
    public QuestManager questManager;

    public AudioSource primaryAudioSource;
    public AudioSource secondaryAudioSource;
    private float volume;

    public AudioClip act1Music;
    public AudioClip act2Music;
    public AudioClip act3Music;
    public AudioClip act4Music;
    public AudioClip finalMusic;

    public AudioClip birdSounds;
    public AudioClip owlSounds;


    private Coroutine musicTransitionCoroutine = null;

    public override void OnNetworkSpawn()
    {
        questManager.currentAct.OnValueChanged += OnActChanged;

        QuestManager.OnQuestComplete += PlayFinalMusicRpc;
        volume = primaryAudioSource.volume;
    }

    public override void OnNetworkDespawn()
    {
        questManager.currentAct.OnValueChanged -= OnActChanged;
        QuestManager.OnQuestComplete -= PlayFinalMusicRpc;         
    }



    private void OnActChanged(int previous, int current)
    {
        SetTrack(current);
    }

    private void SetTrack(int index)
    {
        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
            primaryAudioSource.volume = volume;
        }

        musicTransitionCoroutine = StartCoroutine(MusicTransitionCoroutine(index));
    }

    [Rpc(SendTo.Everyone)]
    private void PlayFinalMusicRpc(int index)
    {
        if (index != 4) return;

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(musicTransitionCoroutine);
            primaryAudioSource.volume = volume;
        }

        primaryAudioSource.Stop();
        secondaryAudioSource.Stop();

        primaryAudioSource.clip = finalMusic;
        primaryAudioSource.Play();
    }

    private IEnumerator MusicTransitionCoroutine(int index)
    {
        Debug.Log($"Transitioning to music for act {index}");

        yield return StartCoroutine(AudioFader.FadeOut(primaryAudioSource, 3f));
        yield return StartCoroutine(AudioFader.FadeOut(secondaryAudioSource, 3f));
        primaryAudioSource.clip = GetClipBySceneIndex(index)[0];
        secondaryAudioSource.clip = GetClipBySceneIndex(index)[1];
        yield return StartCoroutine(AudioFader.FadeIn(primaryAudioSource, 3f));
        yield return StartCoroutine(AudioFader.FadeIn(secondaryAudioSource, 3f));
    }

    private AudioClip[] GetClipBySceneIndex(int index)
    {
        AudioClip[] clip;

        switch (index)
        {
            case 1:
                clip = new[] { act1Music, birdSounds };
                break;
            case 2:
                clip = new[] { act2Music, birdSounds };
                break;
            case 3:
                clip = new[] { act3Music, birdSounds };
                break;
            case 4:
                clip = new[] { act4Music, owlSounds };
                break;
            default:
                clip = null;
                break;
        }

        return clip;
    }
}
