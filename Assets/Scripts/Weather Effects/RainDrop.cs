using Unity.Netcode;
using UnityEngine;

public class RainDrop : NetworkBehaviour
{
    public Rigidbody2D rb;

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector2.down * Time.fixedDeltaTime * 5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        if(collision.gameObject.TryGetComponent(out IRainTarget target))
        {
            target.OnRainHit();
        }

        else Destroy(gameObject);
    }

}

