using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StageLightManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<TimeOfDay> currentTimeOfDay =
        new NetworkVariable<TimeOfDay>(
            TimeOfDay.Twilight
        );

    public TimeOfDay initialTimeOfDay =
        TimeOfDay.Twilight;

    [Header("Lights")]
    public Light2D twilightLight;
    public Light2D morningLight;
    public Light2D dayLight;
    public Light2D eveningLight;
    public Light2D nightLight;

    [Header("Stage Ambience")]
    public AudioSource morningBirds;
    public AudioSource nightOwls;

    public override void OnNetworkSpawn()
    {
        currentTimeOfDay.OnValueChanged +=
            OnTimeOfDayChanged;

        if (IsServer)
        {
            currentTimeOfDay.Value =
                initialTimeOfDay;
        }

        SetLight(currentTimeOfDay.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentTimeOfDay.OnValueChanged -=
            OnTimeOfDayChanged;
    }

    private void OnTimeOfDayChanged(
        TimeOfDay previous,
        TimeOfDay current
    )
    {
        SetLight(current);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance
                .CheckQuestCompletion();
        }
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    public void SetLightRpc(TimeOfDay time)
    {
        if (!IsServer)
        {
            return;
        }

        currentTimeOfDay.Value = time;
    }

    private void SetLight(TimeOfDay time)
    {
        if (twilightLight != null)
        {
            twilightLight.enabled =
                time == TimeOfDay.Twilight;
        }

        if (morningLight != null)
        {
            morningLight.enabled =
                time == TimeOfDay.Morning;
        }

        if (dayLight != null)
        {
            dayLight.enabled =
                time == TimeOfDay.Day;
        }

        if (eveningLight != null)
        {
            eveningLight.enabled =
                time == TimeOfDay.Evening;
        }

        if (nightLight != null)
        {
            nightLight.enabled =
                time == TimeOfDay.Night;
        }

        SetAmbientSound(
            morningBirds,
            time == TimeOfDay.Morning
        );

        SetAmbientSound(
            nightOwls,
            time == TimeOfDay.Night
        );
    }

    private void SetAmbientSound(
        AudioSource source,
        bool shouldPlay
    )
    {
        if (source == null)
        {
            return;
        }

        if (shouldPlay)
        {
            if (!source.isPlaying)
            {
                source.Play();
            }
        }
        else if (source.isPlaying)
        {
            source.Stop();
        }
    }
}