using UnityEngine;
using Unity.Netcode;

public class SimpleFakeSocket : NetworkBehaviour
{
    [Header("Welches Item schaltet das frei?")]
    public string requiredItemName = "VogelAsset"; // Name des Items in der Hand

    [Header("Das feste Ziel-Objekt in der Szene")]
    public GameObject targetVisualObject; // Das bisher unsichtbare Objekt

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    private void Update()
    {
        // Wenn der Spieler in Reichweite ist und "F" drückt (hier Taste per Input oder Standard-Abfrage)
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TrySwapItemServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TrySwapItemServerRpc()
    {
        // 1. Blende das feste Ziel-Objekt für alle Spieler im Netzwerk ein
        if (targetVisualObject != null)
        {
            var netObj = targetVisualObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                // Wenn es ein Netzwerk-Objekt ist, spawnen wir es korrekt oder schalten es um
                // Am einfachsten: Ein ClientRpc zum Aktivieren senden
                ToggleVisualClientRpc(true);
            }
            else
            {
                targetVisualObject.SetActive(true);
            }
        }

        // Hier müsstest du nur noch das getragene Item des Spielers zerstören/verstecken.
    }

    [ClientRpc]
    private void ToggleVisualClientRpc(bool show)
    {
        if (targetVisualObject != null)
            targetVisualObject.SetActive(show);
    }
}