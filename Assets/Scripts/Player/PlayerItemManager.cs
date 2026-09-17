using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerItemManager : NetworkBehaviour
{
    public InputActionReference useControls;
    public Vector2 carryOffset = new(1f, 0f);

    public PlayerMovement playerMovement;
    public PlayerAnimations playerAnimations;

    public List<GameObject> itemsInRange = new();

    public GameObject CarriedItem { get; private set; }

    private Carryable carriedItemScript;
    private Rigidbody2D carriedItemRigidbody;
    private bool hasSyncedCarryPosition;

    public AudioSource audioSource;
    public AudioClip pickupSound;

    private void OnEnable()
    {
        useControls.action.performed += UseItem;
    }

    private void OnDisable()
    {
        useControls.action.performed -= UseItem;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Carryable") &&
            !itemsInRange.Contains(other.gameObject))
        {
            itemsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Carryable"))
        {
            itemsInRange.Remove(other.gameObject);
        }
    }

    public void PickUpItem(GameObject obj)
    {
        if (!IsOwner ||
            CarriedItem != null ||
            obj == null)
        {
            return;
        }

        if (!obj.TryGetComponent(
                out Carryable carryable
            ))
        {
            return;
        }

        if (carryable.isCarried.Value ||
            carryable.isSocketed.Value)
        {
            return;
        }

        if (!obj.TryGetComponent(
                out Rigidbody2D rb
            ))
        {
            return;
        }

        SetCarriedItem(
            obj,
            carryable,
            rb,
            true
        );

        carryable.RequestPickUpServerRpc();
    }

    private void SetCarriedItem(
        GameObject obj,
        Carryable carryable,
        Rigidbody2D rb,
        bool playSound
    )
    {
        CarriedItem = obj;
        carriedItemScript = carryable;
        carriedItemRigidbody = rb;
        hasSyncedCarryPosition = false;

        if (playSound &&
            audioSource != null &&
            pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    public void DropItem()
    {
        if (!IsOwner || CarriedItem == null)
            return;

        if (CarriedItem.TryGetComponent(
                out Carryable carryable
            ))
        {
            carryable.RequestDropServerRpc();
        }

        carriedItemScript = null;
        carriedItemRigidbody = null;
        CarriedItem = null;
        hasSyncedCarryPosition = false;
    }

    private void UseItem(
        InputAction.CallbackContext context
    )
    {
        if (!IsOwner || CarriedItem == null)
            return;

        if (CarriedItem.TryGetComponent(
                out Usable usable
            ))
        {
            usable.OnUse();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || CarriedItem == null)
            return;

        Vector2 currentCarryOffset =
            playerAnimations.isFlipped.Value
                ? new Vector2(
                    -carryOffset.x,
                    carryOffset.y
                )
                : carryOffset;

        carriedItemScript.flip.Value =
            playerAnimations.isFlipped.Value;

        Vector2 carryPosition =
            transform.position +
            (Vector3)currentCarryOffset;

        if (!hasSyncedCarryPosition)
        {
            CarriedItem
                .GetComponent<NetworkTransform>()
                .Teleport(
                    carryPosition,
                    CarriedItem.transform.rotation,
                    CarriedItem.transform.localScale
                );

            hasSyncedCarryPosition = true;
        }
        else
        {
            carriedItemRigidbody.MovePosition(
                carryPosition
            );
        }
    }

    public void InsertIntoSocket(ItemSocket socket)
    {
        if (!IsOwner ||
            CarriedItem == null ||
            socket == null ||
            !socket.IstLeer)
        {
            return;
        }

        NetworkObject itemNetworkObject =
            CarriedItem.GetComponent<NetworkObject>();

        NetworkObject socketNetworkObject =
            socket.GetComponent<NetworkObject>();

        if (itemNetworkObject == null ||
            socketNetworkObject == null)
        {
            return;
        }

        InsertIntoSocketServerRpc(
            itemNetworkObject.NetworkObjectId,
            socketNetworkObject.NetworkObjectId
        );

        carriedItemScript = null;
        carriedItemRigidbody = null;
        CarriedItem = null;
        hasSyncedCarryPosition = false;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Owner
    )]
    private void InsertIntoSocketServerRpc(
        ulong itemId,
        ulong socketId,
        RpcParams rpcParams = default
    )
    {
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

        if (!NetworkManager
                .SpawnManager
                .SpawnedObjects
                .TryGetValue(
                    socketId,
                    out NetworkObject socketObject
                ))
        {
            return;
        }

        ItemSocket socket =
            socketObject.GetComponent<ItemSocket>();

        Carryable carryable =
            itemObject.GetComponent<Carryable>();

        if (socket == null ||
            carryable == null ||
            socket.snapPoint == null ||
            !socket.IstLeer)
        {
            return;
        }

        ulong requestingClientId =
            rpcParams.Receive.SenderClientId;

        if (!carryable.isCarried.Value ||
            carryable.carrierClientId.Value !=
            requestingClientId ||
            carryable.itemCategory !=
            socket.erlaubteKategorie)
        {
            return;
        }

        carryable.isSocketed.Value = true;
        carryable.isCarried.Value = false;
        carryable.carrierClientId.Value =
            ulong.MaxValue;

        if (itemObject.OwnerClientId !=
            NetworkManager.ServerClientId)
        {
            itemObject.RemoveOwnership();
        }

        socket.eingeklinktesItem.Value = itemId;

        itemObject.TrySetParent(
            socketObject.transform,
            true
        );

        NetworkTransform networkTransform =
            itemObject.GetComponent<NetworkTransform>();

        networkTransform.Teleport(
            socket.snapPoint.position,
            socket.snapPoint.rotation,
            itemObject.transform.localScale
        );
    }

    public void TakeFromSocket(ItemSocket socket)
    {
        if (!IsOwner ||
            CarriedItem != null ||
            socket == null ||
            socket.IstLeer)
        {
            return;
        }

        NetworkObject socketNetworkObject =
            socket.GetComponent<NetworkObject>();

        if (socketNetworkObject == null)
            return;

        TakeFromSocketServerRpc(
            socketNetworkObject.NetworkObjectId
        );
    }

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Owner
    )]
    private void TakeFromSocketServerRpc(
        ulong socketId,
        RpcParams rpcParams = default
    )
    {
        if (!NetworkManager
                .SpawnManager
                .SpawnedObjects
                .TryGetValue(
                    socketId,
                    out NetworkObject socketObject
                ))
        {
            return;
        }

        ItemSocket socket =
            socketObject.GetComponent<ItemSocket>();

        if (socket == null || socket.IstLeer)
            return;

        ulong itemId =
            socket.eingeklinktesItem.Value;

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
            !carryable.isSocketed.Value)
        {
            return;
        }

        ulong requestingClientId =
            rpcParams.Receive.SenderClientId;

        itemObject.TryRemoveParent();

        socket.eingeklinktesItem.Value =
            ItemSocket.EmptyItemId;

        carryable.isCarried.Value = true;
        carryable.isSocketed.Value = false;
        carryable.carrierClientId.Value =
            requestingClientId;

        if (itemObject.OwnerClientId !=
            requestingClientId)
        {
            itemObject.ChangeOwnership(
                requestingClientId
            );
        }

        ReceiveSocketItemRpc(
            itemId,
            RpcTarget.Single(
                requestingClientId,
                RpcTargetUse.Temp
            )
        );
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ReceiveSocketItemRpc(
        ulong itemId,
        RpcParams rpcParams = default
    )
    {
        if (!IsOwner || CarriedItem != null)
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

        if (!itemObject.TryGetComponent(
                out Carryable carryable
            ))
        {
            return;
        }

        if (!itemObject.TryGetComponent(
                out Rigidbody2D rb
            ))
        {
            return;
        }

        float rotation = carryable.flip.Value
            ? -carryable.rotationOnPickup
            : carryable.rotationOnPickup;

        itemObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                rotation
            );

        SetCarriedItem(
            itemObject.gameObject,
            carryable,
            rb,
            true
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(
            transform.position +
            (Vector3)carryOffset,
            0.05f
        );
    }
}