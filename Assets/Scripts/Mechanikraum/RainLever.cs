using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RainLever : SimpleInteractible
{
    [Header("Rain System")]
    public RainFunnel funnel;
    public Movable stageCloud;
    public Rain stageRain;

    [Header("Drain")]
    [Min(0.1f)]
    public float drainInterval = 1f;

    [Min(0.01f)]
    public float cloudTargetTolerance = 0.05f;

    [Header("Lever Visuals")]
    public GameObject leverOffVisual;
    public GameObject leverOnVisual;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;

    public NetworkVariable<bool> isActive =
        new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Coroutine drainRoutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            isActive.Value = false;
        }

        isActive.OnValueChanged +=
            HandleActiveChanged;

        RefreshVisuals();
    }

    public override void OnNetworkDespawn()
    {
        isActive.OnValueChanged -=
            HandleActiveChanged;

        base.OnNetworkDespawn();
    }

    public override void OnInteract()
    {
        ToggleRainServerRpc();
    }

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Everyone
    )]
    private void ToggleRainServerRpc()
    {
        if (isActive.Value)
        {
            StopRain();
            return;
        }

        TryStartRain();
    }

    private void TryStartRain()
    {
        if (funnel == null ||
            stageCloud == null ||
            stageRain == null)
        {
            return;
        }

        if (!funnel.IsFull ||
            !IsCloudOnStage())
        {
            return;
        }

        isActive.Value = true;

        if (!stageRain.isRaining.Value)
        {
            stageRain.SetRainRpc(true);
        }

        if (drainRoutine != null)
        {
            StopCoroutine(drainRoutine);
        }

        drainRoutine =
            StartCoroutine(DrainWater());
    }

    private IEnumerator DrainWater()
    {
        while (isActive.Value &&
               funnel.HasWater &&
               IsCloudOnStage())
        {
            yield return new WaitForSeconds(
                drainInterval
            );

            if (!isActive.Value ||
                !IsCloudOnStage())
            {
                break;
            }

            funnel.ConsumeWater();
        }

        drainRoutine = null;
        SetRainActive(false);
    }

    private void StopRain()
    {
        if (drainRoutine != null)
        {
            StopCoroutine(drainRoutine);
            drainRoutine = null;
        }

        SetRainActive(false);
    }

    private void SetRainActive(bool active)
    {
        isActive.Value = active;

        if (stageRain != null &&
            stageRain.isRaining.Value != active)
        {
            stageRain.SetRainRpc(active);
        }
    }

    private bool IsCloudOnStage()
    {
        if (stageCloud == null ||
            stageCloud.PointB == null)
        {
            return false;
        }

        return Vector3.Distance(
            stageCloud.transform.position,
            stageCloud.PointB.position
        ) <= cloudTargetTolerance;
    }

    private void HandleActiveChanged(
        bool previousValue,
        bool currentValue
    )
    {
        RefreshVisuals();

        if (audioSource != null &&
            leverSound != null)
        {
            audioSource.PlayOneShot(
                leverSound
            );
        }
    }

    private void RefreshVisuals()
    {
        if (leverOffVisual != null)
        {
            leverOffVisual.SetActive(
                !isActive.Value
            );
        }

        if (leverOnVisual != null)
        {
            leverOnVisual.SetActive(
                isActive.Value
            );
        }
    }
}