using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightMachineController : MonoBehaviour
{
    [Header("Sockets")]
    public ItemSocket bulbSocket;
    public ItemSocket filterSocket;

    [Header("Stage")]
    public StageLightManager stageLightManager;

    [Header("Machine Lights")]
    public Light2D bulbGlow;
    public Light2D lightCone;

    [Header("Daylight")]
    [ColorUsage(true, true)]
    public Color dayConeColor = Color.white;

    private void OnEnable()
    {
        RefreshMachine();
    }

    private void Update()
    {
        RefreshMachine();
    }

    private void RefreshMachine()
    {
        bool bulbIsInserted =
            bulbSocket != null &&
            !bulbSocket.IstLeer;

        if (bulbGlow != null)
        {
            bulbGlow.enabled =
                bulbIsInserted;
        }

        if (lightCone != null)
        {
            lightCone.enabled =
                bulbIsInserted;
        }

        if (!bulbIsInserted)
            return;

        TimeOfDay selectedTime =
            TimeOfDay.Day;

        Color selectedColor =
            dayConeColor;

        if (filterSocket != null &&
            filterSocket.TryGetSocketedItem(
                out NetworkObject filterObject
            ))
        {
            LightFilter filter =
                filterObject
                    .GetComponent<LightFilter>();

            if (filter != null)
            {
                selectedTime =
                    filter.timeOfDay;

                selectedColor =
                    filter.lightColor;
            }
        }

        if (lightCone != null)
        {
            lightCone.color =
                selectedColor;
        }

        if (stageLightManager == null ||
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (stageLightManager
                .currentTimeOfDay
                .Value != selectedTime)
        {
            stageLightManager.SetLightRpc(
                selectedTime
            );
        }
    }
}