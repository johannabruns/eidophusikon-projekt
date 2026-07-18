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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
