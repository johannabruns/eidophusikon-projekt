using Unity.Netcode;
using UnityEngine;

public class WindAffectable : NetworkBehaviour
{
    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public override void OnNetworkSpawn()
    {
        if (IsServer) Wind.Register(rb);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer) Wind.Unregister(rb);
    }
}