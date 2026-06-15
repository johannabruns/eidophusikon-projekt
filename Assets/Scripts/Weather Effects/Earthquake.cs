using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Earthquake : MonoBehaviour
{
    [Header("Dependencies")]
    public CameraScript cameraScript;
    public AudioSource audioSource;
    public AudioClip earthquakeSound;

    [Header("Configuration")]
    public float duration = 2f;
    public float intensity = 1f;
    public LayerMask ignoreLayers;
    public UnityEvent onActivated;

    public void StartEarthquake()
    {
        //cameraScript.Shake(1f, duration);
        audioSource.PlayOneShot(earthquakeSound);
        StartCoroutine(ShakeObjects());
        onActivated.Invoke();
    }

    public IEnumerator ShakeObjects()
    {
        Rigidbody2D[] rigidbodies = FindRigidBodies();
        float durationElapsed = 0f;
        while (durationElapsed < duration)
        {
            foreach (Rigidbody2D rb in rigidbodies)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * intensity, ForceMode2D.Impulse);
            }
            yield return new WaitForSeconds(0.5f);
            durationElapsed += 0.5f;
        }
    }

    private Rigidbody2D[] FindRigidBodies()
    {
        Rigidbody2D[] rigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        Rigidbody2D[] allRigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        Rigidbody2D[] filteredRigidbodies = System.Array.FindAll(allRigidbodies, rb => (ignoreLayers.value & (1 << rb.gameObject.layer)) == 0);

        System.Array.ForEach(filteredRigidbodies, rb => Debug.Log($"Rigidbody found attached to {rb.gameObject.name}"));

        return filteredRigidbodies;
    }

    public void FixedUpdate()
    {
        
    }
}
