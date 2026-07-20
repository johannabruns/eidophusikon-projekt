using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothSpeed = 5f;

    [Header("Camera Bounds")]
    public PolygonCollider2D boundsCollider;

    void LateUpdate()
    {
        // Wenn noch kein Target vom Spieler übergeben wurde, brechen wir ab und warten.
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (boundsCollider != null)
        {
            Bounds bounds = boundsCollider.bounds;
            Camera cam = GetComponent<Camera>();
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect;

            float clampedX = Mathf.Clamp(desiredPosition.x, bounds.min.x + camHalfWidth, bounds.max.x - camHalfWidth);
            float clampedY = Mathf.Clamp(desiredPosition.y, bounds.min.y + camHalfHeight, bounds.max.y - camHalfHeight);

            desiredPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}