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

        if(collision.gameObject.CompareTag("Flower"))
        {
            GameObject flower = collision.gameObject;
            if (flower.TryGetComponent(out BoolStateObject script))
            {
                script.SetActive();
            }
        }

        else Destroy(gameObject);
    }

}

