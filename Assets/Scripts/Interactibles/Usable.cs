using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public abstract class Usable : Carryable
{
    [SerializeField] protected float range = 1f;
    [SerializeField] protected Animator anim;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip onUseSound;

    public abstract void OnUse();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public bool IsObjectInRange<T>(out T component) where T : NetworkBehaviour
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.gameObject.TryGetComponent(out component))
            {
                return true;
            }
        }
        component = null;
        return false;
    }

    [Rpc(SendTo.Server)]
    protected void RequestObjectDestructionRpc()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(DestroyAfterSound());
        }
        else
            NetworkObject.Despawn();
    }

    private IEnumerator DestroyAfterSound()
    {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitUntil(() => !audioSource.isPlaying);
        NetworkObject.Despawn();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
