using UnityEngine;
using Unity.Netcode;

public class DynamicCameraFollow : MonoBehaviour
{
    [Header("Ziel")]
    public Transform target; 
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [System.Serializable]
    public struct CameraLimits
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }

    [Header("Grenzen für den HOST (Mechanikraum)")]
    public CameraLimits hostLimits;

    [Header("Grenzen für den CLIENT (Bühne)")]
    public CameraLimits clientLimits;

    private Camera cam;
    private CameraLimits activeLimits;
    private CameraLimits targetLimits;
    private bool limitsInitialized = false;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void InitializeLimits()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            activeLimits = hostLimits;
            targetLimits = hostLimits;
        }
        else
        {
            activeLimits = clientLimits;
            targetLimits = clientLimits;
        }
        limitsInitialized = true;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        if (!limitsInitialized)
        {
            InitializeLimits();
            return;
        }

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // --- DIE SCHLEUSEN-LOGIK (Ratchet-System) ---
        
        // 1. Wenn ein Raum GRÖSSER wird (z. B. Flur betreten), machen wir das sofort.
        // Das stört die Kamera nicht, sie kriegt nur mehr Freiraum.
        if (targetLimits.minX < activeLimits.minX) activeLimits.minX = targetLimits.minX;
        if (targetLimits.maxX > activeLimits.maxX) activeLimits.maxX = targetLimits.maxX;
        if (targetLimits.minY < activeLimits.minY) activeLimits.minY = targetLimits.minY;
        if (targetLimits.maxY > activeLimits.maxY) activeLimits.maxY = targetLimits.maxY;

        // 2. Wenn ein Raum KLEINER wird (Tür schließt sich hinter dir), 
        // ziehen wir die Grenze exakt an deiner Kamera-Kante nach, ohne zu schieben!
        float camLeft = transform.position.x - camWidth;
        float camRight = transform.position.x + camWidth;
        float camBottom = transform.position.y - camHeight;
        float camTop = transform.position.y + camHeight;

        if (targetLimits.minX > activeLimits.minX) 
            activeLimits.minX = Mathf.Clamp(camLeft, activeLimits.minX, targetLimits.minX);
            
        if (targetLimits.maxX < activeLimits.maxX) 
            activeLimits.maxX = Mathf.Clamp(camRight, targetLimits.maxX, activeLimits.maxX);
            
        if (targetLimits.minY > activeLimits.minY) 
            activeLimits.minY = Mathf.Clamp(camBottom, activeLimits.minY, targetLimits.minY);
            
        if (targetLimits.maxY < activeLimits.maxY) 
            activeLimits.maxY = Mathf.Clamp(camTop, targetLimits.maxY, activeLimits.maxY);


        // --- NORMALE KAMERA-BEWEGUNG ---
        Vector3 desiredPosition = target.position + offset;
        float clampedX = Mathf.Clamp(desiredPosition.x, activeLimits.minX + camWidth, activeLimits.maxX - camWidth);
        float clampedY = Mathf.Clamp(desiredPosition.y, activeLimits.minY + camHeight, activeLimits.maxY - camHeight);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        transform.position = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
    }

    // Wird vom Trigger an der Tür aufgerufen
    public void UpdateLimits(CameraLimits newLimits)
    {
        targetLimits = newLimits;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}