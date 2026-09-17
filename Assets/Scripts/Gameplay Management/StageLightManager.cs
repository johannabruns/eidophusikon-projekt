using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StageLightManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<TimeOfDay>
        currentTimeOfDay =
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

    [Header("Morning Ambience")]
    public AudioSource morningBirds;

    public override void OnNetworkSpawn()
    {
        currentTimeOfDay.OnValueChanged +=
            OnTimeOfDayChanged;

        if (IsServer)
        {
            currentTimeOfDay.Value =
                initialTimeOfDay;
        }

        SetLight(
            currentTimeOfDay.Value
        );
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
    public void SetLightRpc(
        TimeOfDay time
    )
    {
        if (!IsServer)
            return;

        currentTimeOfDay.Value =
            time;
    }

    private void SetLight(
        TimeOfDay time
    )
    {
        if (twilightLight != null)
        {
            twilightLight.enabled =
                time ==
                TimeOfDay.Twilight;
        }

        if (morningLight != null)
        {
            morningLight.enabled =
                time ==
                TimeOfDay.Morning;
        }

        if (dayLight != null)
        {
            dayLight.enabled =
                time ==
                TimeOfDay.Day;
        }

        if (eveningLight != null)
        {
            eveningLight.enabled =
                time ==
                TimeOfDay.Evening;
        }

        if (nightLight != null)
        {
            nightLight.enabled =
                time ==
                TimeOfDay.Night;
        }

        if (morningBirds == null)
            return;

        bool shouldPlayBirds =
            time ==
            TimeOfDay.Morning;

        if (shouldPlayBirds &&
            !morningBirds.isPlaying)
        {
            morningBirds.Play();
        }
        else if (!shouldPlayBirds &&
                 morningBirds.isPlaying)
        {
            morningBirds.Stop();
        }
    }
}