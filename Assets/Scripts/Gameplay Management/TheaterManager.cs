using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages gameplay elements related to the theater setting, such as curtain animations and audience reactions.
/// </summary>
public class TheaterManager : NetworkBehaviour
{
    private NetworkVariable<bool> initialStateActive = new NetworkVariable<bool>(true);

    public StageLightManager lights;

    public Animator curtainAnim;
    public StageEntrance entrance;

    public AudioSource audioSource;

    public AudioClip longApplause;
    public AudioClip shortApplause;
    public AudioClip audienceChatter;

    private Dictionary<string, AudioClip> soundEffects = new Dictionary<string, AudioClip>();

    public override void OnNetworkSpawn()
    {
        audioSource.loop = true;
        audioSource.clip = audienceChatter;
        audioSource.volume = 0.05f;
        audioSource.Play();

        soundEffects.Add("ShortApplause", shortApplause);
        soundEffects.Add("LongApplause", longApplause);
    }

    public void ShortApplause()     
    {
        PlaySoundRpc("ShortApplause");
    }
    public void LongApplause()
    {

        PlaySoundRpc("LongApplause");
    }

    public void OpenCurtains()
    {
        SetCurtainStateRpc(true);
    }

    public void CloseCurtains()
    {
        SetCurtainStateRpc(false);
    }

    [Rpc(SendTo.Everyone)]
    private void PlaySoundRpc(string sound)
    {
        if (initialStateActive.Value)
        {
            initialStateActive.Value = false;
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.volume = 0.1f;
            audioSource.Stop();
        }

        AudioClip clip = soundEffects[sound];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetCurtainStateRpc(bool state)
    {
        curtainAnim.SetBool("IsOpen", state);
    }
}
