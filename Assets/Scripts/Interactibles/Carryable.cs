using System.Diagnostics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkTransform))]
public class Carryable : NetworkBehaviour
{
    public float rotationOnPickup = 0f; // the rotation to set on the object when it is picked up
    // Server is authoritative over both - clients only ever read these.
    public NetworkVariable<bool> isCarried = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<ulong> carrierClientId = new(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> flip = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;
    private Collider2D col;
    private int uncarriedLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        uncarriedLayer = gameObject.layer;
    }

    public override void OnNetworkSpawn()
    {
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
            rb.simulated = !carried;
        
        if (col != null)
            col.enabled = !carried;

        if(IsOwner)
            transform.rotation = Quaternion.Euler(0f, 0f, rotationOnPickup);
    }

    public void Flip(bool previous, bool current)
    {
        if (current)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            if(IsOwner) transform.rotation = Quaternion.Euler(0f, 0f, -rotationOnPickup);
        }
          
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            if(IsOwner)transform.rotation = Quaternion.Euler(0f, 0f, rotationOnPickup);
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

    private void Update()
    {
        UnityEngine.Debug.Log(transform.rotation.z);
    }
}