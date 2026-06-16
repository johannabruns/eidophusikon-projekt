using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimations : NetworkBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

    private NetworkVariable<bool> networkFlipX = new(false,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
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