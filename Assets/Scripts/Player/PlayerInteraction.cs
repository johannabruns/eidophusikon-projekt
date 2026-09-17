using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Dependencies")]
    public InputActionReference interactionControls;
    public InputActionReference axisControls;

    [Space]
    public PlayerMovement movement;
    public PlayerItemManager itemManager;

    [Header("Configuration")]
    public LayerMask raycastHits;

    [Header("Point-and-Click Pickup")]
    public LayerMask carryableClickLayer;

    [Min(0f)]
    public float maxPointClickDistance = 6f;

    public Interactible CurrentInteractible
    {
        get;
        private set;
    }

    private readonly List<Interactible>
        interactiblesInRange = new();

    public float AxisValue
    {
        get;
        private set;
    }

    private float previousAxisValue;

    private void OnEnable()
    {
        interactionControls.action.performed +=
            ButtonPressInteract;
    }

    private void OnDisable()
    {
        interactionControls.action.performed -=
            ButtonPressInteract;
    }

    private void ButtonPressInteract(
        InputAction.CallbackContext context
    )
    {
        if (!IsOwner)
            return;

        ItemSocket usableSocket =
            FindUsableSocket();

        if (usableSocket != null)
        {
            if (itemManager.CarriedItem != null)
            {
                itemManager.InsertIntoSocket(
                    usableSocket
                );
            }
            else
            {
                itemManager.TakeFromSocket(
                    usableSocket
                );
            }

            return;
        }

        RainFunnel rainFunnel =
            FindRainFunnel();

        if (rainFunnel != null &&
            rainFunnel.AcceptsItem(
                itemManager.CarriedItem
            ))
        {
            rainFunnel.TryDeposit(
                itemManager.CarriedItem
            );

            return;
        }

        if (itemManager.CarriedItem != null)
        {
            itemManager.DropItem();
            return;
        }

        foreach (GameObject nearbyItem
                 in itemManager.itemsInRange)
        {
            if (nearbyItem == null)
                continue;

            bool usesPointAndClick =
                (carryableClickLayer.value &
                 (1 << nearbyItem.layer)) != 0;

            if (usesPointAndClick)
                continue;

            itemManager.PickUpItem(nearbyItem);
            return;
        }

        if (CurrentInteractible
            is SimpleInteractible interactible)
        {
            interactible.OnInteract();
        }
    }

    private ItemSocket FindUsableSocket()
    {
        Carryable carriedItem = null;

        if (itemManager.CarriedItem != null)
        {
            carriedItem =
                itemManager.CarriedItem
                    .GetComponent<Carryable>();
        }

        if (CurrentInteractible
            is ItemSocket currentSocket &&
            CanUseSocket(
                currentSocket,
                carriedItem
            ))
        {
            return currentSocket;
        }

        foreach (Interactible interactible
                 in interactiblesInRange)
        {
            if (interactible
                is ItemSocket socket &&
                CanUseSocket(
                    socket,
                    carriedItem
                ))
            {
                return socket;
            }
        }

        return null;
    }

    private bool CanUseSocket(
        ItemSocket socket,
        Carryable carriedItem
    )
    {
        if (socket == null)
            return false;

        if (carriedItem != null)
        {
            return socket.IstLeer &&
                   carriedItem.itemCategory ==
                   socket.erlaubteKategorie;
        }

        return !socket.IstLeer;
    }

    private RainFunnel FindRainFunnel()
    {
        if (CurrentInteractible
            is RainFunnel currentFunnel)
        {
            return currentFunnel;
        }

        foreach (Interactible interactible
                 in interactiblesInRange)
        {
            if (interactible
                is RainFunnel funnel)
            {
                return funnel;
            }
        }

        return null;
    }

    private void TryPointClickPickup()
    {
        if (!IsOwner)
            return;

        if (itemManager == null ||
            itemManager.CarriedItem != null)
        {
            return;
        }

        if (interactionControls == null ||
            !interactionControls.action.IsPressed())
        {
            return;
        }

        if (Mouse.current == null ||
            !Mouse.current
                .leftButton
                .wasPressedThisFrame)
        {
            return;
        }

        Camera activeCamera = Camera.main;

        if (activeCamera == null)
            return;

        Ray mouseRay =
            activeCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );

        RaycastHit2D[] hits =
            Physics2D.GetRayIntersectionAll(
                mouseRay,
                100f,
                carryableClickLayer
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            Carryable carryable =
                hit.collider
                    .GetComponentInParent<Carryable>();

            if (carryable == null ||
                carryable.isCarried.Value ||
                carryable.isSocketed.Value)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    carryable.transform.position
                );

            if (distance >
                maxPointClickDistance)
            {
                continue;
            }

            itemManager.PickUpItem(
                carryable.gameObject
            );

            return;
        }
    }

    private void AxisInteract(float value)
    {
        if (!IsOwner)
            return;

        if (CurrentInteractible
            is AxisInteractible axisInteractible)
        {
            if (AxisValue == 0f &&
                AxisValue != previousAxisValue)
            {
                axisInteractible.Stop();
                return;
            }

            if (Mouse.current == null ||
                Camera.main == null)
            {
                return;
            }

            Ray ray =
                Camera.main.ScreenPointToRay(
                    Mouse.current.position.ReadValue()
                );

            RaycastHit2D hit =
                Physics2D.GetRayIntersection(
                    ray,
                    50f,
                    raycastHits
                );

            if (AxisValue != 0f &&
                hit.collider != null &&
                hit.collider ==
                axisInteractible.rayTargetCollider)
            {
                axisInteractible.Turn(AxisValue);
            }
        }
    }

    public void AddInteractible(
        Interactible interactible
    )
    {
        if (CurrentInteractible == null)
        {
            SetCurrentInteractible(
                interactible
            );
        }
        else if (
            CurrentInteractible != interactible &&
            !interactiblesInRange.Contains(
                interactible
            ))
        {
            interactiblesInRange.Add(
                interactible
            );
        }
    }

    public void RemoveInteractible(
        Interactible interactible
    )
    {
        if (CurrentInteractible ==
            interactible)
        {
            if (interactiblesInRange.Count == 0)
            {
                CurrentInteractible = null;
                return;
            }

            Interactible newInteractible =
                interactiblesInRange[0];

            interactiblesInRange.RemoveAt(0);

            SetCurrentInteractible(
                newInteractible
            );
        }
        else
        {
            interactiblesInRange.Remove(
                interactible
            );
        }
    }

    private void SetCurrentInteractible(
        Interactible interactible
    )
    {
        CurrentInteractible = interactible;

        if (CurrentInteractible != null &&
            CurrentInteractible.UI != null)
        {
            CurrentInteractible.UI.SetActive(
                true
            );
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        TryPointClickPickup();

        AxisValue =
            axisControls.action.ReadValue<float>();

        AxisInteract(AxisValue);

        previousAxisValue = AxisValue;
    }
}