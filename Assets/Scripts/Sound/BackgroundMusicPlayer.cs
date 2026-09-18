using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BackgroundMusicPlayer : NetworkBehaviour
{
    public QuestManager questManager;

    public AudioSource primaryAudioSource;

    public AudioClip act1Music;
    public AudioClip act2Music;
    public AudioClip act3Music;
    public AudioClip act4Music;
    public AudioClip finalMusic;

    [Min(0f)]
    public float transitionDuration = 3f;

    private Coroutine musicTransitionCoroutine;
    private float normalVolume;

    public override void OnNetworkSpawn()
    {
        if (questManager != null)
        {
            questManager.currentAct.OnValueChanged +=
                OnActChanged;
        }

        QuestManager.OnQuestComplete +=
            PlayFinalMusicRpc;

        if (primaryAudioSource != null)
        {
            normalVolume =
                primaryAudioSource.volume;
        }

        if (questManager != null &&
            questManager.currentAct.Value > 0)
        {
            SetTrack(
                questManager.currentAct.Value
            );
        }
    }

    public override void OnNetworkDespawn()
    {
        if (questManager != null)
        {
            questManager.currentAct.OnValueChanged -=
                OnActChanged;
        }

        QuestManager.OnQuestComplete -=
            PlayFinalMusicRpc;

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(
                musicTransitionCoroutine
            );

            musicTransitionCoroutine = null;
        }
    }

    private void OnActChanged(
        int previous,
        int current
    )
    {
        SetTrack(current);
    }

    private void SetTrack(int actIndex)
    {
        if (primaryAudioSource == null)
        {
            return;
        }

        AudioClip nextClip =
            GetClipByAct(actIndex);

        if (nextClip == null)
        {
            return;
        }

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(
                musicTransitionCoroutine
            );

            primaryAudioSource.volume =
                normalVolume;
        }

        musicTransitionCoroutine =
            StartCoroutine(
                TransitionMusic(nextClip)
            );
    }

    [Rpc(SendTo.Everyone)]
    private void PlayFinalMusicRpc(int actIndex)
    {
        if (actIndex != 4 ||
            primaryAudioSource == null ||
            finalMusic == null)
        {
            return;
        }

        if (musicTransitionCoroutine != null)
        {
            StopCoroutine(
                musicTransitionCoroutine
            );

            musicTransitionCoroutine = null;
        }

        primaryAudioSource.Stop();
        primaryAudioSource.volume =
            normalVolume;

        primaryAudioSource.clip =
            finalMusic;

        primaryAudioSource.Play();
    }

    private IEnumerator TransitionMusic(
        AudioClip nextClip
    )
    {
        if (primaryAudioSource.isPlaying)
        {
            yield return StartCoroutine(
                AudioFader.FadeOut(
                    primaryAudioSource,
                    transitionDuration
                )
            );
        }

        primaryAudioSource.clip =
            nextClip;

        yield return StartCoroutine(
            AudioFader.FadeIn(
                primaryAudioSource,
                transitionDuration,
                normalVolume
            )
        );

        musicTransitionCoroutine = null;
    }

    private AudioClip GetClipByAct(int actIndex)
    {
        return actIndex switch
        {
            1 => act1Music,
            2 => act2Music,
            3 => act3Music,
            4 => act4Music,
            _ => null
        };
    }
}