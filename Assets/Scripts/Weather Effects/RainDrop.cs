using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RainDrop : NetworkBehaviour
{
    public Rigidbody2D rb;
    public List<Sprite> sprites;

    public override void OnNetworkSpawn()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
    }

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

