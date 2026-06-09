using UnityEngine;
using Unity.Netcode;

public class SimplePlayerAnim : NetworkBehaviour
{
    public Animator anim;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    void Update()
    {
        // Nur unser eigener Spieler wird animiert
        if (!IsOwner) return;

        // Wenn wir uns bewegen (Geschwindigkeit > 0.1), spiele die Animation (Speed = 1). 
        // Wenn wir stehen, friere die Animation ein (Speed = 0).
        if (rb.linearVelocity.magnitude > 0.1f)
            anim.speed = 1f;
        else
            anim.speed = 0f;

        // Optional: Das Bild in die richtige Laufrichtung spiegeln
        if (rb.linearVelocity.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (rb.linearVelocity.x < -0.1f)
            spriteRenderer.flipX = true;
    }
}