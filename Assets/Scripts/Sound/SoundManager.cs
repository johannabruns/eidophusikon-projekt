using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public List<AudioSource> machineRoomAudioSources;
    public List<AudioSource> stageAudioSources;

    public override void OnNetworkSpawn()
    {
        SetAudiosources();
    }


    private void SetAudiosources()
    {
        foreach (AudioSource audioSource in machineRoomAudioSources)
        {
            audioSource.enabled = IsServer;
        }

        foreach (AudioSource audioSource in stageAudioSources)
        {
            audioSource.enabled = !IsServer;
        }
    }
}
