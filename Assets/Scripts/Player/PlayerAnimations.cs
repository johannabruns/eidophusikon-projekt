using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;


public class PlayerAnimations : NetworkBehaviour
{
    private PlayerMovement playerMovement;

    private SpriteRenderer spriteRenderer;
    public Sprite playerSprite;
    public Sprite flippedPlayerSprite;

    public NetworkVariable<bool> isFlipped = new(false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // FIX 1: Wir nutzen Awake() statt OnNetworkSpawn(). 
    // Awake feuert sofort in der allerersten Millisekunde, in der das Objekt existiert.
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        isFlipped.OnValueChanged += UpdateSprite;
    }

    public override void OnNetworkDespawn()
    {
        isFlipped.OnValueChanged -= UpdateSprite;
    }

    private void UpdateSprite(bool previous, bool current)
    {
        spriteRenderer.sprite = current ? flippedPlayerSprite : playerSprite;
    }

    void Update()
    {
        // FIX 2: Der Netzwerk-Türsteher. 
        // Bricht Update() sofort ab, wenn der Spieler vom Netzwerk noch nicht freigegeben ist.
        if (!IsSpawned) return;

        if (IsOwner)
        {
            Vector2 movementDirection = playerMovement.MovementDirection;

            //movement direction - update NetworkVariable instead of directly
            if (movementDirection.x != 0)
            {
                isFlipped.Value = movementDirection.x < 0;
            }
        }
    }
}


//LEGACY CODE BELOW - KEEP FOR REFERENCE, BUT NOT USED IN THE PROJECT

/*
public class PlayerAnimations : NetworkBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;


    public NetworkVariable<bool> networkFlipX = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // FIX 1: Wir nutzen Awake() statt OnNetworkSpawn(). 
    // Awake feuert sofort in der allerersten Millisekunde, in der das Objekt existiert.
    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // FIX 2: Der Netzwerk-Türsteher. 
        // Bricht Update() sofort ab, wenn der Spieler vom Netzwerk noch nicht freigegeben ist.
        if (!IsSpawned) return;

        if (IsOwner)
        {
            Vector2 movementDirection = playerMovement.MovementDirection;
            float ladderVertical = playerMovement.LadderVertical;

            //ladder
            if (playerMovement.IsOnLadder && !playerMovement.IsGrounded)
            {
                animator.SetBool("IsClimbing", true);
                animator.SetFloat("ClimbSpeed", ladderVertical);
            }
            else
            {
                animator.SetBool("IsClimbing", false);
            }

            //movement direction - update NetworkVariable instead of directly
            if (movementDirection.x != 0)
            {
                networkFlipX.Value = movementDirection.x < 0;
            }

            //jumping
            if (playerMovement.IsGrounded || playerMovement.IsOnLadder)
            {
                animator.SetBool("IsJumping", false);
            }
            else if (!playerMovement.IsOnLadder)
            {
                animator.SetBool("IsJumping", true);
            }

            //walking
            if (movementDirection == Vector2.zero)
            {
                animator.SetBool("IsWalking", false);
            }
            else
            {
                animator.SetBool("IsWalking", true);
            }
        }

        // Apply flip state on all clients (both owner and non-owners)
        spriteRenderer.flipX = networkFlipX.Value;
    }
}
*/