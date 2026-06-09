

using System;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator animator;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
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

        //movement direction
        if (movementDirection.x != 0)
        {
            spriteRenderer.flipX = movementDirection.x < 0;
        }

        //jumping
        if (playerMovement.IsGrounded || playerMovement.IsOnLadder)
        {
            animator.SetBool("IsJumping", false);
        }
        else if(!playerMovement.IsOnLadder)
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
}