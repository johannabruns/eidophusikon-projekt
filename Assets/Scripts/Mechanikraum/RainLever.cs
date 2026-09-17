using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RainLever : SimpleInteractible
{
    [Header("Rain System")]
    public RainFunnel funnel;
    public Movable stageCloud;
    public Rain stageRain;

    [Header("Cloud Rain Zone")]
    public Transform cloudRainStartPoint;

    [Min(0.01f)]
    public float rainZoneTolerance = 0.15f;

    [Header("Drain")]
    [Min(0.1f)]
    public float drainInterval = 1f;

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
            stageRain == null ||
            cloudRainStartPoint == null)
        {
            return;
        }

        if (!funnel.IsFull ||
            !IsCloudInsideRainZone())
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
               IsCloudInsideRainZone())
        {
            yield return new WaitForSeconds(
                drainInterval
            );

            if (!isActive.Value ||
                !IsCloudInsideRainZone())
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

    private bool IsCloudInsideRainZone()
    {
        if (stageCloud == null ||
            stageCloud.PointB == null ||
            cloudRainStartPoint == null)
        {
            return false;
        }

        Vector2 cloudPosition =
            stageCloud.transform.position;

        Vector2 zoneStart =
            cloudRainStartPoint.position;

        Vector2 zoneEnd =
            stageCloud.PointB.position;

        Vector2 zoneDirection =
            zoneEnd - zoneStart;

        float zoneLengthSquared =
            zoneDirection.sqrMagnitude;

        if (zoneLengthSquared <= 0.0001f)
            return false;

        float progress =
            Vector2.Dot(
                cloudPosition - zoneStart,
                zoneDirection
            ) / zoneLengthSquared;

        if (progress < 0f ||
            progress > 1f)
        {
            return false;
        }

        Vector2 closestPoint =
            zoneStart +
            zoneDirection * progress;

        float distanceFromZone =
            Vector2.Distance(
                cloudPosition,
                closestPoint
            );

        return distanceFromZone <=
               rainZoneTolerance;
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