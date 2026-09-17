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

    [Header("Unfiltered Colors")]
    [ColorUsage(true, true)]
    public Color twilightConeColor =
        new Color(
            0.55f,
            0.35f,
            0.45f,
            1f
        );

    [ColorUsage(true, true)]
    public Color dayConeColor =
        Color.white;

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
        {
            ApplyStageTime(
                TimeOfDay.Twilight
            );

            return;
        }

        TimeOfDay selectedTime =
            GetUnfilteredTime();

        Color selectedColor =
            selectedTime ==
            TimeOfDay.Twilight
                ? twilightConeColor
                : dayConeColor;

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

        ApplyStageTime(
            selectedTime
        );
    }

    private TimeOfDay GetUnfilteredTime()
    {
        if (QuestManager.Instance == null)
        {
            return TimeOfDay.Twilight;
        }

        int actIndex =
            QuestManager.Instance
                .currentAct
                .Value;

        if (actIndex <= 1)
        {
            return TimeOfDay.Twilight;
        }

        return TimeOfDay.Day;
    }

    private void ApplyStageTime(
        TimeOfDay selectedTime
    )
    {
        if (stageLightManager == null ||
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (stageLightManager
                .currentTimeOfDay
                .Value ==
            selectedTime)
        {
            return;
        }

        stageLightManager.SetLightRpc(
            selectedTime
        );
    }
}