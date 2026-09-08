using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class CollisionEvent : MonoBehaviour
{
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    [Tooltip("If true, only reacts when the colliding object is the local player's own NetworkObject.")]
    public bool onlyLocalPlayer = true;
    public bool onlyReactToPlayers = true;
    public bool destroyOnEnter = false;

    private bool IsRelevant(Component collision)
    {
        if (!onlyLocalPlayer) return true;

        var netObj = collision.GetComponentInParent<NetworkObject>();
        // If it's a networked object, only react if it belongs to the local player.
        // Non-networked colliders (props, etc.) still pass through.
        return netObj == null || netObj.IsOwner;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsRelevant(collision)) return;
        if (onlyReactToPlayers)
        {
            if(!collision.gameObject.CompareTag("Player"))      
                return;           
        }

        OnEnter.Invoke();

        if (destroyOnEnter)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsRelevant(collision)) return;
        if (onlyReactToPlayers)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;
        }

        OnExit.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsRelevant(collision.collider)) return;
        if (onlyReactToPlayers)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;
        }

        OnEnter.Invoke();

        if (destroyOnEnter)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsRelevant(collision.collider)) return;
        if (onlyReactToPlayers)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;
        }

        OnExit.Invoke();
    }
}