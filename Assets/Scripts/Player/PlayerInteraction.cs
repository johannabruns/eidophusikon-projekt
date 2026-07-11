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

    [Header("Configuration")]
    public Vector2 carryOffset = new(1, 0); //the position at which the carried item should be held relative to the player
    public LayerMask raycastHits;

    public Interactible CurrentInteractible { get; private set; } // the item the player is currently able to interact with
    private List<Interactible> interactiblesInRange = new(); //all interactibles that are currently in range of the player (excluding the current interactible)

    private GameObject carriedItem = null; //The item currently being carried by the player
    private Rigidbody2D carriedItemRb = null; //The rigidbody of the currently carried item
    private int carriedItemLayer; //The layer the currently carried item is on before being picked up

    public bool IsCarryingItem => carriedItem != null;

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

        if (CurrentInteractible is SimpleInteractible interactible)
            interactible.OnInteract();
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

    public void PickUpItem(GameObject obj)
    {
        if (carriedItem != null) return;

        carriedItem = obj;

        //temporarily set the layer of the carried item to defalt
        carriedItemLayer = carriedItem.layer;
        carriedItem.layer = LayerMask.NameToLayer("Default");

        if (obj.TryGetComponent(out Rigidbody2D rb))
        {
            carriedItemRb = rb;

            //reset and freeze rotation off the carried item
            rb.rotation = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (obj.TryGetComponent(out Collider2D col))
            //diasable collision to avoid physics issues
            col.enabled = false;
    }

    public void DropItem(GameObject obj)
    {
        if (obj != carriedItem) return;

        carriedItemRb.constraints = RigidbodyConstraints2D.None;
        carriedItem.layer = carriedItemLayer;
        carriedItemRb = null;

        if (obj.TryGetComponent(out Collider2D col))
            col.enabled = true;

        carriedItem = null;
    }

    void Update()
    {
        AxisValue = axisControls.action.ReadValue<float>();
        AxisInteract(AxisValue);
        previousAxisValue = AxisValue;
    }

    private void FixedUpdate()
    {
        float horizontalMovement = playerMovement.MovementDirection.x;

        Vector2 carryPos;

        if (horizontalMovement != 0)
        {
            // Use absolute value and apply the correct sign
            carryOffset = new Vector2(Mathf.Abs(carryOffset.x) * Mathf.Sign(horizontalMovement), carryOffset.y);
        }

        carryPos = transform.position + (Vector3)carryOffset;

        if (carriedItemRb != null)
            carriedItemRb.MovePosition(carryPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + (Vector3)carryOffset, 0.05f);
    }
}

