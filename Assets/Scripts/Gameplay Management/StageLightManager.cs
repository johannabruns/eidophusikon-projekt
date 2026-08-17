using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StageLightManager : NetworkBehaviour
{
    public NetworkVariable<TimeOfDay> currentTimeOfDay = new NetworkVariable<TimeOfDay>(TimeOfDay.Morning);
    public TimeOfDay initialTimeOfDay;

    public Light2D morningLight;
    public Light2D dayLight;
    public Light2D eveningLight;
    public Light2D nightLight;

    private void Awake()
    {
        currentTimeOfDay.Value = initialTimeOfDay;
    }

    public override void OnNetworkSpawn()
    {
        currentTimeOfDay.OnValueChanged += OnTimeOfDayChanged;
        SetLight(currentTimeOfDay.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentTimeOfDay.OnValueChanged -= OnTimeOfDayChanged;
    }

    private void OnTimeOfDayChanged(TimeOfDay previous, TimeOfDay current)
    {
        SetLight(current);
    }

    [Rpc(SendTo.Server)]
    public void SetLightRpc(TimeOfDay time)
    {
        if (!IsServer) return;
        currentTimeOfDay.Value = time;
    }

    private void SetLight(TimeOfDay time)
    {
        morningLight.enabled = time == TimeOfDay.Morning;
        dayLight.enabled = time == TimeOfDay.Day;
        eveningLight.enabled = time == TimeOfDay.Evening;
        nightLight.enabled = time == TimeOfDay.Night;
    }
    
}
