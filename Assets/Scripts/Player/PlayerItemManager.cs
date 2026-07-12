using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This sits on a child of the PlayerObject, so it shares the PlayerObject's
// NetworkObject - IsOwner here means "this is the local player who owns the
// character", same as it does on PlayerInteraction.
public class PlayerItemManager : NetworkBehaviour
{
    public Vector2 carryOffset = new(1, 0); //the position at which the carried item should be held relative to the player
    public PlayerMovement playerMovement;

    public List<GameObject> itemsInRange = new(); //all items that are currently in range of the player

    public GameObject CarriedItem { get; private set; } = null; //The item currently being carried by the player

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
        if (!IsOwner) return; // only the local player who pressed the button should ever call this
        if (CarriedItem != null) return;
        if (!obj.TryGetComponent(out Carryable carryable)) return;
        if (carryable.isCarried.Value) return;

        // Optimistically remember it locally so movement/UI can react
        // immediately. The server is still the one deciding whether the
        // pickup actually goes through - Carryable applies the resulting
        // physics/visual state itself via its NetworkVariable callback.
        CarriedItem = obj;
        carryable.RequestPickUpServerRpc();
    }

    public void DropItem()
    {
        if (!IsOwner) return;
        if (CarriedItem == null) return;

        if (CarriedItem.TryGetComponent(out Carryable carryable))
            carryable.RequestDropServerRpc();

        CarriedItem = null;
    }

    private void Update()
    {
        // No longer physics-driven (Carryable disables the rigidbody's
        // simulation while carried), so this can run every frame instead of
        // every fixed step, and just push the transform directly.
        if (CarriedItem == null || !IsOwner) return;

        float horizontalMovement = playerMovement.MovementDirection.x;

        if (horizontalMovement != 0)
            carryOffset = new Vector2(Mathf.Abs(carryOffset.x) * Mathf.Sign(horizontalMovement), carryOffset.y);

        Vector2 carryPos = transform.position + (Vector3)carryOffset;
        CarriedItem.transform.position = carryPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + (Vector3)carryOffset, 0.05f);
    }
}