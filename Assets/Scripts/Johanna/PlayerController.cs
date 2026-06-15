using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    private Rigidbody2D rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // DIE RETTUNG FÜR DEN CLIENT:
        // Wenn uns diese Spielfigur NICHT gehört, stellen wir sie auf Kinematic.
        // Dadurch berechnet dieser Laptop keine eigene Schwerkraft für den fremden Spieler
        // und vertraut rein auf die Positionen, die über das Netzwerk reinkommen!
        if (!IsOwner)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // Wenn uns dieser Spieler gehört, schnappen wir uns die Hauptkamera
        if (IsOwner)
        {
            // Der Host (Server) startet exakt auf deinen Koordinaten
            if (IsServer)
            {
                transform.position = new Vector3(-6.51f, 2.31f, 0f); 
            }
            // Der Client startet ein bisschen weiter rechts, damit sie nicht ineinander stecken!
            else
            {
                transform.position = new Vector3(-0.61f, -3.36f, 0f); 
            }

            // Kamera-Code (die fehlerhafte Doppelung wurde entfernt):
            NetworkCameraFollow camFollow = Camera.main.GetComponent<NetworkCameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(this.transform);
            }
        }
    }

    private void Update()
    {
        // Nur der echte Besitzer des Spielers darf die Tasten drücken
        if (!IsOwner) return;

        // 1. Links / Rechts Bewegung
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Wir speichern kurz, ob wir auf dem Boden sind (macht den Code lesbarer)
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        // 2. Sprung-Logik (Leertaste oder W)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }
}