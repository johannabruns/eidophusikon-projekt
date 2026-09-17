using Unity.Netcode;
using UnityEngine;

public class PlayerAnimations : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;

    public Sprite playerSprite;
    public Sprite flippedPlayerSprite;

    public NetworkVariable<bool> isFlipped =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    private void Awake()
    {
        playerMovement =
            GetComponent<PlayerMovement>();

        spriteRenderer =
            GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        isFlipped.OnValueChanged +=
            UpdateSprite;

        ApplySprite(
            isFlipped.Value
        );
    }

    public override void OnNetworkDespawn()
    {
        isFlipped.OnValueChanged -=
            UpdateSprite;
    }

    private void Update()
    {
        if (!IsSpawned ||
            !IsOwner ||
            playerMovement == null ||
            playerMovement
                .FinaleMovementInProgress)
        {
            return;
        }

        Vector2 movementDirection =
            playerMovement.MovementDirection;

        if (movementDirection.x != 0f)
        {
            isFlipped.Value =
                movementDirection.x < 0f;
        }
    }

    public void FaceLeft()
    {
        if (!IsOwner)
            return;

        isFlipped.Value = true;
        ApplySprite(true);
    }

    public void FaceRight()
    {
        if (!IsOwner)
            return;

        isFlipped.Value = false;
        ApplySprite(false);
    }

    private void UpdateSprite(
        bool previous,
        bool current
    )
    {
        ApplySprite(current);
    }

    private void ApplySprite(
        bool flipped
    )
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite =
            flipped
                ? flippedPlayerSprite
                : playerSprite;
    }
}