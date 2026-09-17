using System;
using Unity.Netcode;
using UnityEngine;

public class ItemSocket : SimpleInteractible
{
    public const ulong EmptyItemId = ulong.MaxValue;

    [Header("Socket Settings")]
    public ItemCategory erlaubteKategorie;
    public Transform snapPoint;

    public NetworkVariable<ulong> eingeklinktesItem = new(
        EmptyItemId,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IstLeer =>
        eingeklinktesItem.Value == EmptyItemId;

    public event Action<ulong, ulong> ItemChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            eingeklinktesItem.Value = EmptyItemId;
        }

        eingeklinktesItem.OnValueChanged += HandleItemChanged;
    }

    public override void OnNetworkDespawn()
    {
        eingeklinktesItem.OnValueChanged -= HandleItemChanged;
    }

    private void HandleItemChanged(
        ulong previousItem,
        ulong currentItem
    )
    {
        ItemChanged?.Invoke(
            previousItem,
            currentItem
        );
    }

    public bool TryGetSocketedItem(
        out NetworkObject item
    )
    {
        item = null;

        if (IstLeer || NetworkManager == null)
            return false;

        return NetworkManager
            .SpawnManager
            .SpawnedObjects
            .TryGetValue(
                eingeklinktesItem.Value,
                out item
            );
    }

    public override void OnInteract()
    {
    }
}