using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Usable : Carryable
{
    [SerializeField] protected float range = 1f;
    [SerializeField] protected Animator anim;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip onUseSound;

    private bool consumptionStarted;

    public abstract void OnUse();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        consumptionStarted = false;
    }

    public bool IsObjectInRange<T>(
        out T component
    ) where T : NetworkBehaviour
    {
        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                transform.position,
                range
            );

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject &&
                collider.gameObject.TryGetComponent(
                    out component
                ))
            {
                return true;
            }
        }

        component = null;
        return false;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    protected void RequestObjectDestructionRpc(
        float delay,
        RpcParams rpcParams = default
    )
    {
        if (consumptionStarted ||
            NetworkObject == null ||
            !NetworkObject.IsSpawned)
        {
            return;
        }

        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        bool senderCarriesItem =
            isCarried.Value &&
            carrierClientId.Value ==
            senderClientId;

        bool senderOwnsItem =
            NetworkObject.OwnerClientId ==
            senderClientId;

        if (!senderCarriesItem &&
            !senderOwnsItem)
        {
            return;
        }

        consumptionStarted = true;

        HideConsumedObjectRpc();

        StartCoroutine(
            DespawnAfterDelay(
                Mathf.Max(0f, delay)
            )
        );
    }

    [Rpc(SendTo.Everyone)]
    private void HideConsumedObjectRpc()
    {
        foreach (
            SpriteRenderer renderer
            in GetComponentsInChildren<
                SpriteRenderer
            >(true)
        )
        {
            renderer.enabled = false;
        }

        foreach (
            Collider2D itemCollider
            in GetComponentsInChildren<
                Collider2D
            >(true)
        )
        {
            itemCollider.enabled = false;
        }

        foreach (
            Rigidbody2D itemBody
            in GetComponentsInChildren<
                Rigidbody2D
            >(true)
        )
        {
            itemBody.simulated = false;
        }
    }

    private IEnumerator DespawnAfterDelay(
        float delay
    )
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                delay
            );
        }

        if (NetworkObject != null &&
            NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            range
        );
    }
}
