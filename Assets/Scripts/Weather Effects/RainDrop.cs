using Unity.Netcode;
using UnityEngine;

public class RainDrop : NetworkBehaviour
{
    public Rigidbody2D rb;

    private void FixedUpdate()
    {
        if (!IsServer) return;
        rb.MovePosition(rb.position + Vector2.down * Time.fixedDeltaTime * 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.isTrigger) return;

        if(collision.gameObject.TryGetComponent(out IRainTarget target))
        {
            target.OnRainHit();
        }

        else 
            NetworkObject.Despawn();
    }

}

