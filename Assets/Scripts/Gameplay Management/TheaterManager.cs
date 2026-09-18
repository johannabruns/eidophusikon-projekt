using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages gameplay elements related to the theater setting, such as curtain animations and audience reactions.
/// </summary>
public class TheaterManager : NetworkBehaviour
{
    private bool initialAudienceChatterActive = true;

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
        initialAudienceChatterActive = true;
        soundEffects.Clear();

        soundEffects["ShortApplause"] = shortApplause;
        soundEffects["LongApplause"] = longApplause;

        if (audioSource == null)
        {
            return;
        }

        audioSource.loop = true;
        audioSource.clip = audienceChatter;
        audioSource.volume = 0.02f;

        if (audienceChatter != null)
        {
            audioSource.Play();
        }
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
        if (audioSource == null)
        {
            return;
        }

        if (initialAudienceChatterActive)
        {
            initialAudienceChatterActive = false;
            audioSource.loop = false;
            audioSource.clip = null;
            audioSource.volume = 0.1f;
            audioSource.Stop();
        }

        if (soundEffects.TryGetValue(
                sound,
                out AudioClip clip
            ) &&
            clip != null)
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
