using UnityEngine;

public class NetworkCameraFollow : MonoBehaviour
{
    private Transform target;

    // Diese 4 Felder tauchen gleich im Unity Inspector auf!
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Wir berechnen die Wunsch-Position der Kamera
            float targetX = target.position.x;
            float targetY = target.position.y;

            // Hier sperren wir die Koordinaten ein!
            // Wenn targetX kleiner als minX ist, wird es auf minX gesetzt usw.
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            float clampedY = Mathf.Clamp(targetY, minY, maxY);

            // Setze die Kamera auf die eingesperrte Position (Z bleibt -10)
            transform.position = new Vector3(clampedX, clampedY, -10f);
        }
    }
}