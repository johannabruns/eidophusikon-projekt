using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    // Durch das "Instance" können wir von jedem anderen Skript aus den Befehl zum Wackeln geben
    public static CameraShaker Instance; 

    private float shakeTimeRemaining;
    private float shakePower;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float power)
    {
        shakeTimeRemaining = duration;
        shakePower = power;
    }

    private void LateUpdate()
    {
        // Solange noch Zeit auf der Wackel-Uhr ist...
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;
            
            // ... berechnen wir eine zufällige kleine Verschiebung ...
            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;
            
            // ... und addieren sie auf die aktuelle Kameraposition
            transform.position += new Vector3(xAmount, yAmount, 0f);
        }
    }
}