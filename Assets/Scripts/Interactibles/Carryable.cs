using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A component that allows a GameObject to be picked up and carried by a player.
/// </summary>
/// 

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkTransform))]
public class Carryable : NetworkBehaviour
{
    [Header("Item Typ")]
    public ItemCategory itemCategory = ItemCategory.None;
    
    public float rotationOnPickup = 0f; // the rotation to set on the object when it is picked up

    // Server is authoritative over both - clients only ever read these.
    public NetworkVariable<bool> isCarried = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<ulong> carrierClientId = new(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> flip = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;
    private Collider2D col;
    private int uncarriedLayer;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        uncarriedLayer = gameObject.layer;
        originalScale = transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        gameObject.tag = "Carryable";

        NetworkTransform networkTransform = GetComponent<NetworkTransform>();
        networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;

        isCarried.OnValueChanged += OnCarriedChanged;
        flip.OnValueChanged += Flip;

        // In case this client joins late / this object spawns already carried,
        // make sure the visual state matches immediately rather than waiting
        // for the next change event.
        ApplyCarriedState(isCarried.Value);
    }

    public override void OnNetworkDespawn()
    {
        isCarried.OnValueChanged -= OnCarriedChanged;
        flip.OnValueChanged -= Flip;
    }

    private void OnCarriedChanged(bool previous, bool current)
    {
        ApplyCarriedState(current);
    }

    private void ApplyCarriedState(bool carried)
    {
        gameObject.layer = carried ? LayerMask.NameToLayer("Default") : uncarriedLayer;

        if (rb != null)
        {
            rb.bodyType = carried ? RigidbodyType2D.Kinematic: RigidbodyType2D.Dynamic;

            if(carried)
            {
                rb.angularVelocity = 0f;
                rb.linearVelocity = Vector2.zero;
            }
        }

        // Exclude the player layer from the collider when carried to prevent collisions with the player
        if (col != null)
            col.excludeLayers = carried ? LayerMask.GetMask("Player") : 0;
    }

    public void Flip(bool previous, bool current)
    {
        float xScale = Mathf.Abs(originalScale.x);

        if (current)
        {
            transform.localScale = new Vector3(-xScale, originalScale.y, originalScale.z);
            if (IsOwner) transform.rotation = Quaternion.Euler(0f, 0f, -rotationOnPickup);
        }
        else
        {
            transform.localScale = new Vector3(xScale, originalScale.y, originalScale.z);
            if (IsOwner) transform.rotation = Quaternion.Euler(0f, 0f, rotationOnPickup);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPickUpServerRpc(RpcParams rpcParams = default)
    {
        if (isCarried.Value) return;

        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        isCarried.Value = true;
        carrierClientId.Value = requestingClientId;

        NetworkObject.ChangeOwnership(requestingClientId);
        ApplyPickupRotationRpc(RpcTarget.Single(requestingClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestDropServerRpc(RpcParams rpcParams = default)
    {
        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        // Only the current carrier is allowed to drop it
        if (!isCarried.Value || carrierClientId.Value != requestingClientId) return;

        isCarried.Value = false;
        carrierClientId.Value = ulong.MaxValue;

        if (NetworkObject.OwnerClientId != NetworkManager.ServerClientId)
            NetworkObject.RemoveOwnership();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ApplyPickupRotationRpc(RpcParams rpcParams = default)
    {
        float rotation = flip.Value ? -rotationOnPickup : rotationOnPickup;
        transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        Physics2D.SyncTransforms();
    }
}