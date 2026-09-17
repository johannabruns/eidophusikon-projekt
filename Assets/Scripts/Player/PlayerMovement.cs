using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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

    public bool IsGrounded
    {
        get;
        private set;
    }

    public bool IsOnLadder
    {
        get;
        private set;
    }

    public bool FinaleMovementInProgress
    {
        get;
        private set;
    }

    private float gravityScale;
    private Coroutine finaleMovement;

    public Vector2 MovementDirection
    {
        get;
        private set;
    }

    public float LadderVertical
    {
        get;
        private set;
    }

    public override void OnNetworkSpawn()
    {
        gravityScale =
            rigidBody.gravityScale;

        if (IsOwner)
        {
            CameraScript camScript =
                Camera.main.GetComponent<CameraScript>();

            camScript.EnterPlayerFollowMode(
                transform
            );
        }
    }

    private void OnEnable()
    {
        jumpControls.action.performed +=
            Jump;
    }

    private void OnDisable()
    {
        jumpControls.action.performed -=
            Jump;
    }

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (!IsOwner ||
            FinaleMovementInProgress)
        {
            return;
        }

        if (collision.gameObject.CompareTag(
                "Ladder"
            ))
        {
            IsOnLadder = true;
            rigidBody.gravityScale = 0f;
        }
    }

    private void OnTriggerExit2D(
        Collider2D collision
    )
    {
        if (!IsOwner ||
            FinaleMovementInProgress)
        {
            return;
        }

        if (collision.gameObject.CompareTag(
                "Ladder"
            ))
        {
            IsOnLadder = false;

            rigidBody.gravityScale =
                gravityScale;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (FinaleMovementInProgress)
        {
            MovementDirection =
                Vector2.zero;

            LadderVertical = 0f;
            return;
        }

        MovementDirection =
            movementControls.action
                .ReadValue<Vector2>();

        LadderVertical =
            ladderControls.action
                .ReadValue<float>();

        IsGrounded =
            GroundCheck();
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (FinaleMovementInProgress)
        {
            rigidBody.linearVelocity =
                Vector2.zero;

            return;
        }

        rigidBody.linearVelocity =
            new Vector2(
                MovementDirection.x *
                moveSpeed,
                rigidBody.linearVelocity.y
            );

        if (IsOnLadder)
        {
            rigidBody.linearVelocity =
                new Vector2(
                    rigidBody.linearVelocity.x,
                    LadderVertical *
                    moveSpeed *
                    0.75f
                );
        }
    }

    private void Jump(
        InputAction.CallbackContext obj
    )
    {
        if (!IsOwner ||
            FinaleMovementInProgress)
        {
            return;
        }

        if (IsGrounded &&
            !IsOnLadder)
        {
            rigidBody.linearVelocity =
                new Vector2(
                    rigidBody.linearVelocity.x,
                    jumpForce
                );
        }
    }

    public bool GroundCheck()
    {
        return Physics2D.OverlapBox(
            groundCheckPoint.position,
            groundCheckSize,
            0f,
            groundLayer
        );
    }

    [Rpc(SendTo.Everyone)]
    public void MoveToFinaleRpc(
        Vector3 worldPosition,
        float duration
    )
    {
        if (!IsOwner)
            return;

        if (finaleMovement != null)
        {
            StopCoroutine(
                finaleMovement
            );
        }

        finaleMovement =
            StartCoroutine(
                MoveToFinaleRoutine(
                    worldPosition,
                    duration
                )
            );
    }

    private IEnumerator MoveToFinaleRoutine(
        Vector3 worldPosition,
        float duration
    )
    {
        FinaleMovementInProgress = true;
        MovementDirection = Vector2.zero;
        LadderVertical = 0f;
        IsOnLadder = false;

        rigidBody.linearVelocity =
            Vector2.zero;

        rigidBody.angularVelocity = 0f;
        rigidBody.gravityScale = 0f;
        rigidBody.simulated = false;

        Vector3 startPosition =
            transform.position;

        float safeDuration =
            Mathf.Max(
                0.01f,
                duration
            );

        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed /
                    safeDuration
                );

            float smoothProgress =
                progress *
                progress *
                (3f - 2f * progress);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    worldPosition,
                    smoothProgress
                );

            yield return null;
        }

        transform.position =
            worldPosition;

        rigidBody.simulated = true;

        rigidBody.position =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        rigidBody.linearVelocity =
            Vector2.zero;

        rigidBody.angularVelocity = 0f;
        rigidBody.gravityScale =
            gravityScale;

        PlayerAnimations animations =
            GetComponent<PlayerAnimations>();

        if (animations != null)
        {
            animations.FaceLeft();
        }

        FinaleMovementInProgress = false;
        finaleMovement = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            groundCheckPoint.position,
            groundCheckSize
        );
    }
}