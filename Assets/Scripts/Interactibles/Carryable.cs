using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Carryable : NetworkBehaviour
{
    // Server is authoritative over both - clients only ever read these.
    public NetworkVariable<bool> isCarried = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<ulong> carrierClientId = new(ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        isCarried.OnValueChanged += OnCarriedChanged;

        // In case this client joins late / this object spawns already carried,
        // make sure the visual state matches immediately rather than waiting
        // for the next change event.
        ApplyCarriedState(isCarried.Value);
    }

    public override void OnNetworkDespawn()
    {
        isCarried.OnValueChanged -= OnCarriedChanged;
    }

    private void OnCarriedChanged(bool previous, bool current)
    {
        ApplyCarriedState(current);
    }

    private void ApplyCarriedState(bool carried)
    {
        gameObject.layer = carried ? LayerMask.NameToLayer("Default") : uncarriedLayer;

        if (col != null)
            col.enabled = !carried;

        if (rb != null)
        {
            if (carried) rb.rotation = 0;
            rb.simulated = !carried;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestPickUpServerRpc(RpcParams rpcParams = default)
    {
        if (isCarried.Value) return; // someone already has it

        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        isCarried.Value = true;
        carrierClientId.Value = requestingClientId;

        // Hand ownership of this NetworkObject to the carrier. 
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
}