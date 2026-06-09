using UnityEngine;

public class WindEffect : MonoBehaviour
{
    [Header("Wind Einstellungen")]
    public float windSpeed = 15f; 
    
    [Tooltip("Wie viele Meter soll der Wind nach rechts fliegen, bevor er verschwindet?")]
    public float travelDistance = 40f; 

    private Vector3 startPosition;
    private bool isBlowing = false;

    private void Awake()
    {
        startPosition = transform.position; 
        gameObject.SetActive(false); 
    }

    public void BlowWind()
    {
        transform.position = startPosition; 
        gameObject.SetActive(true);         
        isBlowing = true;                   
    }

    private void Update()
    {
        if (isBlowing)
        {
            // 1. Wind nach rechts schieben
            float newX = transform.position.x + (windSpeed * Time.deltaTime);
            float newY = startPosition.y + Mathf.Sin(Time.time * 5f) * 0.5f;

            transform.position = new Vector3(newX, newY, transform.position.z);

            // 2. NEU: Prüfen, ob die Reisestrecke (Startpunkt + Distanz) erreicht wurde
            if (transform.position.x > startPosition.x + travelDistance)
            {
                isBlowing = false;
                gameObject.SetActive(false);
            }
        }
    }
}