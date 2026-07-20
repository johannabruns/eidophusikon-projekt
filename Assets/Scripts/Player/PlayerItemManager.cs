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
    private Rigidbody2D carriedItemRigidbody = null; //The Rigidbody2D component of the item currently being carried by the player

    private bool hasSyncedCarryPosition = false;
    private bool lastSentFlip = false; // Merkt sich den letzten Flip-Zustand, um das Netzwerk nicht zu spammen

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

        if (!obj.TryGetComponent(out Rigidbody2D rb)) return;

        CarriedItem = obj;
        carriedItemScript = carryable;
        carriedItemRigidbody = rb;

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

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        //calculate carry position based on whether the player is facing left or right
        Vector2 carryOffset = playerAnimations.networkFlipX.Value ?
            new(-this.carryOffset.x, this.carryOffset.y) : this.carryOffset;

        if (CarriedItem == null) return;

        bool currentFlip = playerAnimations.networkFlipX.Value;

        // --- FLIP LOGIK (Sicher für Client & Server) ---
        if (IsServer)
        {
            // Der Server darf die Variable direkt setzen
            carriedItemScript.flip.Value = currentFlip;
        }
        else if (currentFlip != lastSentFlip)
        {
            // Der Client bittet den Server per Rpc, den Wert zu ändern (nur wenn er sich ändert!)
            UpdateFlipServerRpc(CarriedItem.GetComponent<NetworkObject>().NetworkObjectId, currentFlip);
            lastSentFlip = currentFlip;
        }

        Vector2 carryPos = transform.position + (Vector3)carryOffset;

        // --- TELEPORT LOGIK (Sicher für Client & Server) ---
        if (!hasSyncedCarryPosition)
        {
            if (IsServer)
            {
                // Der Server darf direkt teleportieren
                CarriedItem.GetComponent<NetworkTransform>().Teleport(carryPos, CarriedItem.transform.rotation, CarriedItem.transform.localScale);
            }
            else
            {
                // Der Client schickt den Teleport-Befehl an den Server
                TeleportItemServerRpc(CarriedItem.GetComponent<NetworkObject>().NetworkObjectId, carryPos);
            }
            
            hasSyncedCarryPosition = true;
        }
        else
        {
            // Die normale Physik-Bewegung läuft weiter
            carriedItemRigidbody.MovePosition(carryPos);
        }
    }

    [ServerRpc]
    private void UpdateFlipServerRpc(ulong itemId, bool flipValue)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject itemObj))
        {
            if (itemObj.TryGetComponent(out Carryable carryable))
            {
                carryable.flip.Value = flipValue;
            }
        }
    }

    [ServerRpc]
    private void TeleportItemServerRpc(ulong itemId, Vector2 pos)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject itemObj))
        {
            if (itemObj.TryGetComponent(out NetworkTransform netTransform))
            {
                netTransform.Teleport(pos, itemObj.transform.rotation, itemObj.transform.localScale);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + (Vector3)carryOffset, 0.05f);
    }

    // ==========================================
    // --- NEUE SOCKET LOGIK ---
    // ==========================================

    public void InsertIntoSocket(ItemSocket socket)
    {
        if (!IsOwner || CarriedItem == null) return;

        NetworkObject itemNetObj = CarriedItem.GetComponent<NetworkObject>();
        NetworkObject socketNetObj = socket.GetComponent<NetworkObject>();

        // 1. Lokale Variablen sofort leeren (damit das Item nicht mehr an der Hand klebt)
        carriedItemScript = null;
        CarriedItem = null;

        // 2. Den Server bitten, das Item physisch ins Socket zu stecken
        InsertIntoSocketServerRpc(itemNetObj.NetworkObjectId, socketNetObj.NetworkObjectId);
    }

    [ServerRpc]
    private void InsertIntoSocketServerRpc(ulong itemId, ulong socketId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject itemObj) &&
            NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(socketId, out NetworkObject socketObj))
        {
            ItemSocket socket = socketObj.GetComponent<ItemSocket>();
            Carryable carryable = itemObj.GetComponent<Carryable>();

            // Lokis "Drop" Logik simulieren, damit es keinem Spieler mehr gehört
            carryable.isCarried.Value = false;
            carryable.carrierClientId.Value = ulong.MaxValue;
            itemObj.RemoveOwnership();

            // Im Socket registrieren
            socket.eingeklinktesItem.Value = itemId;

            // Das Item wird ein Child des Sockets, damit es sich (z.B. am Seil) perfekt mitbewegt!
            itemObj.TrySetParent(socketObj.transform, false);

            // Position & Rotation exakt auf den SnapPoint setzen
            itemObj.transform.position = socket.snapPoint.position;
            itemObj.transform.rotation = socket.snapPoint.rotation;
        }
    }

    public void TakeFromSocket(ItemSocket socket)
    {
        if (!IsOwner || CarriedItem != null) return;

        NetworkObject socketNetObj = socket.GetComponent<NetworkObject>();
        ulong itemId = socket.eingeklinktesItem.Value;

        // 1. Den Server bitten, das Item vom SnapPoint zu lösen
        TakeFromSocketServerRpc(socketNetObj.NetworkObjectId);

        // 2. Wir heben es direkt lokal auf, indem wir Lokis bestehende Funktion nutzen!
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject itemObj))
        {
            PickUpItem(itemObj.gameObject);
        }
    }

    [ServerRpc]
    private void TakeFromSocketServerRpc(ulong socketId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(socketId, out NetworkObject socketObj))
        {
            ItemSocket socket = socketObj.GetComponent<ItemSocket>();
            ulong itemId = socket.eingeklinktesItem.Value;

            if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemId, out NetworkObject itemObj))
            {
                // Vom Socket entkoppeln (entfernt das Child-Parent-Verhältnis)
                socket.eingeklinktesItem.Value = 0;
                itemObj.TryRemoveParent();
            }
        }
    }
}