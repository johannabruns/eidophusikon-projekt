using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WindDrum : Interactible
{
    [Header("Wind")]
    public Wind stageWind;

    [Header("Drum")]
    public Collider2D rubArea;
    public Transform rubLeftPoint;
    public Transform rubRightPoint;

    [Header("Mallet")]
    public GameObject rubMalletVisual;
    public ItemCategory requiredCategory =
        ItemCategory.Trommelschlaegel;

    public float malletRotation = 0f;

    [Header("Rubbing")]
    [Min(0.01f)]
    public float distanceToStartWind = 0.8f;

    [Min(0f)]
    public float minimumMovementPerFrame = 0.005f;

    [Min(0.05f)]
    public float stopDelay = 0.5f;

    private GameObject activeCarriedItem;

    private readonly List<SpriteRenderer> hiddenRenderers =
        new();

    private readonly List<bool> previousRendererStates =
        new();

    private bool isRubbing;
    private bool windRequested;
    private bool hasLastMalletPosition;

    private Vector2 lastMalletPosition;
    private float accumulatedDistance;
    private float lastMovementTime;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (rubMalletVisual != null)
        {
            rubMalletVisual.SetActive(false);
        }
    }

    private void Update()
    {
        PlayerItemManager itemManager =
            FindLocalItemManager();

        if (!CanRub(itemManager))
        {
            EndRubbing();
            return;
        }

        if (Mouse.current == null ||
            Camera.main == null)
        {
            EndRubbing();
            return;
        }

        Vector3 mouseWorldPosition =
            Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        mouseWorldPosition.z =
            rubLeftPoint.position.z;

        bool mousePressed =
            Mouse.current.leftButton.isPressed;

        bool mouseInsideRubArea =
            rubArea.OverlapPoint(
                mouseWorldPosition
            );

        if (!mousePressed ||
            !mouseInsideRubArea)
        {
            EndRubbing();
            return;
        }

        Vector2 malletPosition =
            CalculateMalletPosition(
                mouseWorldPosition
            );

        if (!isRubbing)
        {
            BeginRubbing(
                itemManager,
                malletPosition
            );
        }

        UpdateMalletVisual(
            malletPosition
        );

        MeasureRubbingMovement(
            malletPosition
        );
    }

    private PlayerItemManager FindLocalItemManager()
    {
        if (player == null)
            return null;

        PlayerItemManager itemManager =
            player.GetComponent<PlayerItemManager>();

        if (itemManager == null)
        {
            itemManager =
                player.GetComponentInChildren
                    <PlayerItemManager>();
        }

        if (itemManager == null)
        {
            itemManager =
                player.GetComponentInParent
                    <PlayerItemManager>();
        }

        if (itemManager == null ||
            !itemManager.IsOwner)
        {
            return null;
        }

        return itemManager;
    }

    private bool CanRub(
        PlayerItemManager itemManager
    )
    {
        if (itemManager == null ||
            itemManager.CarriedItem == null ||
            rubArea == null ||
            rubLeftPoint == null ||
            rubRightPoint == null ||
            rubMalletVisual == null ||
            stageWind == null)
        {
            return false;
        }

        Carryable carryable =
            itemManager.CarriedItem
                .GetComponent<Carryable>();

        return carryable != null &&
               carryable.itemCategory ==
               requiredCategory;
    }

    private Vector2 CalculateMalletPosition(
        Vector2 mouseWorldPosition
    )
    {
        Vector2 leftPosition =
            rubLeftPoint.position;

        Vector2 rightPosition =
            rubRightPoint.position;

        Vector2 rubLine =
            rightPosition - leftPosition;

        float lineLengthSquared =
            rubLine.sqrMagnitude;

        if (lineLengthSquared <= 0.0001f)
        {
            return leftPosition;
        }

        float progress =
            Vector2.Dot(
                mouseWorldPosition -
                leftPosition,
                rubLine
            ) / lineLengthSquared;

        progress = Mathf.Clamp01(progress);

        return Vector2.Lerp(
            leftPosition,
            rightPosition,
            progress
        );
    }

    private void BeginRubbing(
        PlayerItemManager itemManager,
        Vector2 malletPosition
    )
    {
        isRubbing = true;
        activeCarriedItem =
            itemManager.CarriedItem;

        accumulatedDistance = 0f;
        lastMalletPosition = malletPosition;
        hasLastMalletPosition = true;
        lastMovementTime = Time.time;

        HideCarriedMallet();

        rubMalletVisual.SetActive(true);
    }

    private void UpdateMalletVisual(
        Vector2 malletPosition
    )
    {
        Vector3 targetPosition =
            malletPosition;

        targetPosition.z =
            rubMalletVisual.transform.position.z;

        rubMalletVisual.transform.position =
            targetPosition;

        rubMalletVisual.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                malletRotation
            );
    }

    private void MeasureRubbingMovement(
        Vector2 malletPosition
    )
    {
        if (!hasLastMalletPosition)
        {
            lastMalletPosition =
                malletPosition;

            hasLastMalletPosition = true;
            return;
        }

        float movementDistance =
            Vector2.Distance(
                lastMalletPosition,
                malletPosition
            );

        lastMalletPosition =
            malletPosition;

        if (movementDistance >=
            minimumMovementPerFrame)
        {
            accumulatedDistance +=
                movementDistance;

            lastMovementTime = Time.time;
        }

        if (!windRequested &&
            accumulatedDistance >=
            distanceToStartWind)
        {
            windRequested = true;
            stageWind.StartWindRpc();
        }

        if (windRequested &&
            Time.time - lastMovementTime >=
            stopDelay)
        {
            windRequested = false;
            accumulatedDistance = 0f;
            stageWind.StopWindRpc();
        }
    }

    private void HideCarriedMallet()
    {
        hiddenRenderers.Clear();
        previousRendererStates.Clear();

        if (activeCarriedItem == null)
            return;

        SpriteRenderer[] renderers =
            activeCarriedItem
                .GetComponentsInChildren
                    <SpriteRenderer>(true);

        foreach (
            SpriteRenderer spriteRenderer
            in renderers
        )
        {
            hiddenRenderers.Add(
                spriteRenderer
            );

            previousRendererStates.Add(
                spriteRenderer.enabled
            );

            spriteRenderer.enabled = false;
        }
    }

    private void RestoreCarriedMallet()
    {
        for (
            int i = 0;
            i < hiddenRenderers.Count;
            i++
        )
        {
            if (hiddenRenderers[i] != null)
            {
                hiddenRenderers[i].enabled =
                    previousRendererStates[i];
            }
        }

        hiddenRenderers.Clear();
        previousRendererStates.Clear();
    }

    private void EndRubbing()
    {
        if (!isRubbing &&
            !windRequested)
        {
            return;
        }

        if (windRequested &&
            stageWind != null)
        {
            stageWind.StopWindRpc();
        }

        RestoreCarriedMallet();

        if (rubMalletVisual != null)
        {
            rubMalletVisual.SetActive(false);
        }

        isRubbing = false;
        windRequested = false;
        hasLastMalletPosition = false;
        accumulatedDistance = 0f;
        activeCarriedItem = null;
    }

    private void OnDisable()
    {
        RestoreCarriedMallet();

        if (rubMalletVisual != null)
        {
            rubMalletVisual.SetActive(false);
        }
    }
}