using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Movable))]
public class Star : BoolStateObject
{
    public Transform targetPosition;
    public NetworkVariable<bool> reachedPosition = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        reachedPosition.OnValueChanged += OnReachedPositionChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        reachedPosition.OnValueChanged -= OnReachedPositionChanged;
    }

    private void OnReachedPositionChanged(bool previous, bool current)
    {
        SetActive(current);
    }

    private void Update()
    { 
        if(IsServer)
        {
            if (Vector2.Distance(transform.position, targetPosition.position) < 0.1f)
            {
                reachedPosition.Value = true;
            }
            else
            {
                reachedPosition.Value = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (targetPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition.position, 0.1f);
        }
    }
}
