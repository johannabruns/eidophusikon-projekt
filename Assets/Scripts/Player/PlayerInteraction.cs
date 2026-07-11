using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Dependencies")]
    public InputActionReference buttonPressControls;
    public InputActionReference axisControls;
    public PlayerMovement playerMovement;
    public PlayerItemManager playerItemManager;

    [Header("Configuration")]
    public LayerMask raycastHits;

    public Interactible CurrentInteractible { get; private set; } // the item the player is currently able to interact with
    private List<Interactible> interactiblesInRange = new(); //all interactibles that are currently in range of the player (excluding the current interactible)

    public float AxisValue { get; private set; }
    private float previousAxisValue = 0f;


    private void OnEnable()
    {
        buttonPressControls.action.performed += ButtonPressInteract;
    }
    private void OnDisable()
    {
        buttonPressControls.action.performed -= ButtonPressInteract;
    }

    /// <summary>
    /// Manages interaction with simple interactibles
    /// </summary>
    /// <param name="obj"></param>
    private void ButtonPressInteract(InputAction.CallbackContext obj)
    {
        if (!IsOwner) return;

        if(playerItemManager.CarriedItem != null)
        {
            playerItemManager.DropItem();
            return;
        }

        if (playerItemManager.itemsInRange.Count > 0)
        {
            playerItemManager.PickUpItem(playerItemManager.itemsInRange[0]);
            return;
        }

        if (CurrentInteractible is SimpleInteractible interactible)
        {
            interactible.OnInteract();
        }
    }

    /// <summary>
    /// Manages interaction with interactibles that require axis input
    /// </summary>
    /// <param name="val"></param>
    private void AxisInteract(float val)
    {
        if (!IsOwner) return;

        if (CurrentInteractible is Wheel wheel)
        {
            if (AxisValue == 0 && AxisValue != previousAxisValue)
            {
                Debug.Log("Stopping wheel");
                wheel.Stop();
                return;
            }

            // Create a ray from the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            // Raycast into the 2D scene
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 50f, raycastHits);

            // Check if the collider hit is the wheel's collider
            if (AxisValue != 0 && hit.collider != null && hit.collider == wheel.wheelCollider)
            {
                Debug.Log("Turning Wheel");
                wheel.Turn(AxisValue);
            }
        }
    }

    /// <summary>
    /// Sets the current interactible or adds one to the list of interactibles in range if there is already a current interactible
    /// </summary>
    /// <param name="interactible"></param>
    public void AddInteractible(Interactible interactible)
    {
        if (CurrentInteractible == null)
            SetCurrentInteractible(interactible);

        else if (CurrentInteractible != interactible)
            interactiblesInRange.Add(interactible);
    }

    /// <summary>
    /// Clears the current interactible. Sets a new current interactible, if there are any within range.
    /// </summary>
    /// <param name="interactible"></param>
    public void RemoveInteractible(Interactible interactible)
    {
        if (CurrentInteractible == interactible)
        {
            if (interactiblesInRange.Count == 0)
            {
                CurrentInteractible = null;
                return;
            }

            Interactible newInteractible = interactiblesInRange.Count > 0 ? interactiblesInRange[0] : null;
            interactiblesInRange.Remove(newInteractible);
            SetCurrentInteractible(newInteractible);
        }
        else if (interactiblesInRange.Contains(interactible))
        {
            interactiblesInRange.Remove(interactible);
        }
    }

    private void SetCurrentInteractible(Interactible interactible)
    {
        CurrentInteractible = interactible;
        if (CurrentInteractible != null && CurrentInteractible.UI != null)
            CurrentInteractible.UI.SetActive(true);
    }

    void Update()
    {
        AxisValue = axisControls.action.ReadValue<float>();
        AxisInteract(AxisValue);
        previousAxisValue = AxisValue;
    }
}

