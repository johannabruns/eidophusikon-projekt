using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour 
{
    [Header("Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Depedencies")]
    public Rigidbody2D rigidBody;
    public InputActionReference movementControls;
    public InputActionReference jumpControls;
    public InputActionReference ladderControls;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public Vector2 groundCheckSize;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool IsOnLadder { get; private set; }
    private float gravityScale;

    public Vector2 MovementDirection { get; private set; }
    public float LadderVertical { get; private set; }

    public override void OnNetworkSpawn()
    {
        gravityScale = rigidBody.gravityScale;

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

            // NEU: Hier sucht der Spieler jetzt nach unserem neuen "DynamicCameraFollow"-Skript!
            DynamicCameraFollow camFollow = Camera.main.GetComponent<DynamicCameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(this.transform);
            }
        }
    }

    private void OnEnable()
    {
        jumpControls.action.performed += Jump;
    }
    private void OnDisable()
    {
        jumpControls.action.performed -= Jump;
    }

    private void Update()
    {
        if (!IsOwner) return;

        MovementDirection = movementControls.action.ReadValue<Vector2>();
        LadderVertical = ladderControls.action.ReadValue<float>();
        IsGrounded = GroundCheck();
    }

    void FixedUpdate()
    {
        if(IsOwner)
        {
            rigidBody.linearVelocity = new Vector2(MovementDirection.x * moveSpeed, rigidBody.linearVelocity.y);

            if(IsOnLadder)
            {
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, LadderVertical * moveSpeed * 0.75f);
            }
        }
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (IsGrounded && !IsOnLadder)
                rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, jumpForce);
        }
    }

    public bool GroundCheck()
    {
        return Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Ladder"))
        {
            IsOnLadder = true;
            rigidBody.gravityScale = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            IsOnLadder = false;
            rigidBody.gravityScale = gravityScale;
        }
    }
}
