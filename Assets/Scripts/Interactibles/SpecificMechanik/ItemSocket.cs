using Unity.Netcode;
using UnityEngine;

public class ItemSocket : SimpleInteractible
{
    [Header("Steckplatz Einstellungen")]
    public ItemCategory erlaubteKategorie; // Was darf hier rein?
    public Transform snapPoint;            // Wo genau soll das Item einrasten?

    // Speichert die NetworkObjectId des Items, das gerade drinsteckt (0 = leer)
    public NetworkVariable<ulong> eingeklinktesItem = new NetworkVariable<ulong>(0);

    public bool IstLeer => eingeklinktesItem.Value == 0;

    public override void OnInteract()
    {
        // Wir lassen diese Methode leer! 
        // Warum? Weil der Spieler das Socket nicht "benutzt", 
        // sondern der PlayerItemManager übergibt das Item an das Socket.
        // Die Logik dafür bauen wir gleich im Spieler-Skript ein.
    }
}