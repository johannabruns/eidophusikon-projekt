using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class RainFunnel : SimpleInteractible
{
    [Header("Water")]
    [Min(1)]
    public int requiredDrops = 5;

    public NetworkVariable<int> currentDrops =
        new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [Header("Stop Motion Visuals")]
    public GameObject[] dropVisuals;

    [Header("Reusable Water Item")]
    public Transform waterSourceReturnPoint;

    public bool IsFull =>
        currentDrops.Value >=
        Mathf.Max(1, requiredDrops);

    public bool HasWater =>
        currentDrops.Value > 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentDrops.Value = 0;
        }

        currentDrops.OnValueChanged +=
            HandleWaterLevelChanged;

        RefreshVisuals();
    }

    public override void OnNetworkDespawn()
    {
        currentDrops.OnValueChanged -=
            HandleWaterLevelChanged;

        base.OnNetworkDespawn();
    }

    public bool AcceptsItem(GameObject item)
    {
        if (item == null)
            return false;

        Carryable carryable =
            item.GetComponent<Carryable>();

        return carryable != null &&
               carryable.itemCategory ==
               ItemCategory.Wassertropfen;
    }

    public void TryDeposit(GameObject item)
    {
        if (!AcceptsItem(item) ||
            IsFull)
        {
            return;
        }

        NetworkObject itemNetworkObject =
            item.GetComponent<NetworkObject>();

        if (itemNetworkObject == null)
            return;

        DepositDropServerRpc(
            itemNetworkObject.NetworkObjectId
        );
    }

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Everyone
    )]
    private void DepositDropServerRpc(
        ulong itemId,
        RpcParams rpcParams = default
    )
    {
        if (IsFull)
            return;

        if (!NetworkManager
                .SpawnManager
                .SpawnedObjects
                .TryGetValue(
                    itemId,
                    out NetworkObject itemObject
                ))
        {
            return;
        }

        Carryable carryable =
            itemObject.GetComponent<Carryable>();

        if (carryable == null ||
            carryable.itemCategory !=
            ItemCategory.Wassertropfen)
        {
            return;
        }

        ulong requestingClientId =
            rpcParams.Receive.SenderClientId;

        if (!carryable.isCarried.Value ||
            carryable.carrierClientId.Value !=
            requestingClientId)
        {
            return;
        }

        currentDrops.Value =
            Mathf.Min(
                currentDrops.Value + 1,
                requiredDrops
            );

        if (IsFull)
        {
            ReturnWaterItemToSource(
                itemObject,
                carryable,
                requestingClientId
            );
        }
    }

    private void ReturnWaterItemToSource(
        NetworkObject itemObject,
        Carryable carryable,
        ulong requestingClientId
    )
    {
        carryable.isCarried.Value = false;
        carryable.isSocketed.Value = false;
        carryable.carrierClientId.Value =
            ulong.MaxValue;

        if (itemObject.OwnerClientId !=
            NetworkManager.ServerClientId)
        {
            itemObject.RemoveOwnership();
        }

        if (itemObject.TryGetComponent(
                out Rigidbody2D rb
            ))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (waterSourceReturnPoint != null &&
            itemObject.TryGetComponent(
                out NetworkTransform networkTransform
            ))
        {
            networkTransform.Teleport(
                waterSourceReturnPoint.position,
                waterSourceReturnPoint.rotation,
                itemObject.transform.localScale
            );
        }

        if (NetworkManager
                .ConnectedClients
                .TryGetValue(
                    requestingClientId,
                    out NetworkClient networkClient
                ) &&
            networkClient.PlayerObject != null)
        {
            PlayerItemManager itemManager =
                networkClient.PlayerObject
                    .GetComponentInChildren<
                        PlayerItemManager
                    >(true);

            if (itemManager != null)
            {
                itemManager
                    .ClearReturnedCarryableRpc(
                        itemObject.NetworkObjectId
                    );
            }
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
        if (dropVisuals == null)
            return;

        for (int i = 0;
             i < dropVisuals.Length;
             i++)
        {
            if (dropVisuals[i] != null)
            {
                dropVisuals[i].SetActive(
                    i < currentDrops.Value
                );
            }
        }
    }

    public bool ConsumeWater()
    {
        if (!IsServer ||
            currentDrops.Value <= 0)
        {
            return false;
        }

        currentDrops.Value--;
        return true;
    }

    public void EmptyFunnel()
    {
        if (!IsServer)
            return;

        currentDrops.Value = 0;
    }

    public override void OnInteract()
    {
    }
}