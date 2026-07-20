using Unity.Netcode;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraAssigner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Prüfen, ob wir der lokale Spieler an diesem PC sind
        if (IsOwner)
        {
            // Suche die Cinemachine-Kamera in der Szene
            CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();
            if (cam != null)
            {
                // Setze uns selbst als Follow-Ziel für die Kamera
                cam.Follow = transform;
            }
        }
    }
}