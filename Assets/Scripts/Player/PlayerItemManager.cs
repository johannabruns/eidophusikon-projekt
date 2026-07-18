using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// This sits on a child of the PlayerObject, so it shares the PlayerObject's
// NetworkObject - IsOwner here means "this is the local player who owns the
// character", same as it does on PlayerInteraction.
public class PlayerItemManager : NetworkBehaviour
{
    public InputActionReference useControls;
    public Vector2 carryOffset = new(1, 0); //the position at which the carried item should be held relative to the player

    public PlayerMovement playerMovement;
    public PlayerAnimations playerAnimations;

    public List<GameObject> itemsInRange = new(); //all items that are currently in range of the player

    public GameObject CarriedItem { get; private set; } = null; //The item currently being carried by the player
    private Carryable carriedItemScript = null; //The Carryable component of the item currently being carried by the player

    private bool hasSyncedCarryPosition = false;

    private void OnEnable()
    {
        useControls.action.performed += UseItem;
    }

    private void OnDisable()
    {
        useControls.action.performed -= UseItem;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Carryable"))
        {
            itemsInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Carryable") && itemsInRange.Contains(other.gameObject))
        {
            itemsInRange.Remove(other.gameObject);
        }
    }

    public void PickUpItem(GameObject obj)
    {
        if (!IsOwner) return;
        if (CarriedItem != null) return;

        if (!obj.TryGetComponent(out Carryable carryable)) return;
        if (carryable.isCarried.Value) return;

        CarriedItem = obj;
        carriedItemScript = carryable;
        hasSyncedCarryPosition = false;
        carryable.RequestPickUpServerRpc();
    }

    public void DropItem()
    {
        if (!IsOwner) return;
        if (CarriedItem == null) return;

        if (CarriedItem.TryGetComponent(out Carryable carryable))
            carryable.RequestDropServerRpc();

        carriedItemScript = null;
        CarriedItem = null;
    }

    private void UseItem(InputAction.CallbackContext obj)
    {
        if (!IsOwner) return;
        if (CarriedItem == null) return;

        if (CarriedItem.TryGetComponent(out Usable usable))
            usable.OnUse();
    }

    private void Update()
    {
        if (!IsOwner) return;

        //calculate carry position based on whether the player is facing left or right
        Vector2 carryOffset = playerAnimations.networkFlipX.Value ?
            new(-this.carryOffset.x, this.carryOffset.y) : this.carryOffset;

        if (CarriedItem == null) return;

        //flip the carried item to match the player's facing direction
        carriedItemScript.flip.Value = playerAnimations.networkFlipX.Value;

        Vector2 carryPos = transform.position + (Vector3)carryOffset;

        if (!hasSyncedCarryPosition)
        {
            CarriedItem.GetComponent<NetworkTransform>().Teleport(carryPos, CarriedItem.transform.rotation, CarriedItem.transform.localScale);
            hasSyncedCarryPosition = true;
        }
        else
        {
            CarriedItem.transform.position = carryPos;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + (Vector3)carryOffset, 0.05f);
    }
}