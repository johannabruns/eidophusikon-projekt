using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RainFunnel : ItemSocket
{
    [Header("Water")]
    [Min(1)]
    public int requiredDrops = 3;

    public NetworkVariable<int> currentDrops =
        new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Visuals")]
    public GameObject emptyVisual;
    public GameObject fullVisual;

    public bool IsFull =>
        currentDrops.Value >= requiredDrops;

    public bool HasWater =>
        currentDrops.Value > 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ItemChanged += HandleSocketItemChanged;
        currentDrops.OnValueChanged +=
            HandleWaterLevelChanged;

        RefreshVisuals();
    }

    public override void OnNetworkDespawn()
    {
        ItemChanged -= HandleSocketItemChanged;
        currentDrops.OnValueChanged -=
            HandleWaterLevelChanged;

        base.OnNetworkDespawn();
    }

    private void HandleSocketItemChanged(
        ulong previousItem,
        ulong currentItem
    )
    {
        if (!IsServer ||
            currentItem == EmptyItemId)
        {
            return;
        }

        StartCoroutine(
            ConsumeDropNextFrame(currentItem)
        );
    }

    private IEnumerator ConsumeDropNextFrame(
        ulong itemId
    )
    {
        yield return null;

        if (!NetworkManager
                .SpawnManager
                .SpawnedObjects
                .TryGetValue(
                    itemId,
                    out NetworkObject itemObject
                ))
        {
            eingeklinktesItem.Value =
                EmptyItemId;

            yield break;
        }

        Carryable carryable =
            itemObject.GetComponent<Carryable>();

        if (carryable == null ||
            carryable.itemCategory !=
            ItemCategory.Wassertropfen)
        {
            eingeklinktesItem.Value =
                EmptyItemId;

            yield break;
        }

        currentDrops.Value =
            Mathf.Min(
                currentDrops.Value + 1,
                requiredDrops
            );

        eingeklinktesItem.Value =
            EmptyItemId;

        if (itemObject.IsSpawned)
        {
            itemObject.Despawn(true);
        }
    }

    private void HandleWaterLevelChanged(
        int previousValue,
        int currentValue
    )
    {
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (emptyVisual != null)
        {
            emptyVisual.SetActive(!IsFull);
        }

        if (fullVisual != null)
        {
            fullVisual.SetActive(IsFull);
        }
    }

    public bool ConsumeWater()
    {
        if (!IsServer || currentDrops.Value <= 0)
            return false;

        currentDrops.Value--;
        return true;
    }

    public void EmptyFunnel()
    {
        if (!IsServer)
            return;

        currentDrops.Value = 0;
    }
}