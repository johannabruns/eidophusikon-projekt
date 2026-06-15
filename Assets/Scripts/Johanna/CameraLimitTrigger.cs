using UnityEngine;
using Unity.Netcode;

public class CameraLimitTrigger : MonoBehaviour
{
    [Header("Die neuen Grenzen für DIESEN Raum")]
    public DynamicCameraFollow.CameraLimits newRoomLimits;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Prüfen: Ist das Objekt, das hier durchläuft, als "Player" markiert?
        if (collision.CompareTag("Player"))
        {
            // 2. Prüfen: Ist das UNSER Spieler? (Wir ignorieren fremde Spielfiguren über das Netzwerk)
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                // 3. Dem Kamera-Skript die neuen Grenzen dieses Raumes in die Hand drücken!
                DynamicCameraFollow camFollow = Camera.main.GetComponent<DynamicCameraFollow>();
                if (camFollow != null)
                {
                    camFollow.UpdateLimits(newRoomLimits);
                    Debug.Log("Kamera-Grenzen wurden für den neuen Raum überschrieben!");
                }
            }
        }
    }
}