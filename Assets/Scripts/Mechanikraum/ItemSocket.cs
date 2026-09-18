using System;
using Unity.Netcode;
using UnityEngine;

public class ItemSocket : SimpleInteractible
{
    public const ulong EmptyItemId =
        ulong.MaxValue;

    [Header("Socket Settings")]
    public ItemCategory erlaubteKategorie;
    public Transform snapPoint;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip insertSound;
    public AudioClip removeSound;

    public NetworkVariable<ulong> eingeklinktesItem =
        new(
            EmptyItemId,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IstLeer =>
        eingeklinktesItem.Value ==
        EmptyItemId;

    public event Action<ulong, ulong>
        ItemChanged;

    private bool audioReady;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            eingeklinktesItem.Value =
                EmptyItemId;
        }

        eingeklinktesItem.OnValueChanged +=
            HandleItemChanged;

        audioReady = true;
    }

    public override void OnNetworkDespawn()
    {
        audioReady = false;

        eingeklinktesItem.OnValueChanged -=
            HandleItemChanged;

        base.OnNetworkDespawn();
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

        if (!audioReady ||
            audioSource == null)
        {
            return;
        }

        bool itemWasInserted =
            previousItem == EmptyItemId &&
            currentItem != EmptyItemId;

        bool itemWasRemoved =
            previousItem != EmptyItemId &&
            currentItem == EmptyItemId;

        if (itemWasInserted &&
            insertSound != null)
        {
            audioSource.PlayOneShot(
                insertSound
            );
        }
        else if (itemWasRemoved &&
                 removeSound != null)
        {
            audioSource.PlayOneShot(
                removeSound
            );
        }
    }

    public bool TryGetSocketedItem(
        out NetworkObject item
    )
    {
        item = null;

        if (IstLeer ||
            NetworkManager == null)
        {
            return false;
        }

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