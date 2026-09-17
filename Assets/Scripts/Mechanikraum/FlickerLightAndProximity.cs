using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickerLight2D : MonoBehaviour
{
    [Header("Light")]
    public Light2D targetLight;
    public GameObject litVisual;

    [Header("Fade")]
    [Min(0.01f)]
    public float fadeSpeed = 5f;

    [Header("Flicker")]
    public bool useFlicker = true;

    [Min(0f)]
    public float minimumFlicker = 0.75f;

    [Min(0f)]
    public float maximumFlicker = 1.05f;

    [Min(0.01f)]
    public float flickerSpeed = 8f;

    private readonly HashSet<Collider2D>
        playersInside = new();

    private float baseIntensity;
    private float flickerSeed;

    private void Awake()
    {
        if (targetLight == null)
            return;

        baseIntensity =
            targetLight.intensity;

        flickerSeed =
            Random.Range(0f, 1000f);

        targetLight.intensity = 0f;
        targetLight.enabled = false;

        if (litVisual != null)
        {
            litVisual.SetActive(false);
        }
    }

    private void Update()
    {
        if (targetLight == null)
            return;

        playersInside.RemoveWhere(
            playerCollider =>
                playerCollider == null
        );

        bool shouldBeActive =
            playersInside.Count > 0;

        float targetIntensity = 0f;

        if (shouldBeActive)
        {
            float flickerMultiplier = 1f;

            if (useFlicker)
            {
                float noise =
                    Mathf.PerlinNoise(
                        flickerSeed,
                        Time.time *
                        flickerSpeed
                    );

                flickerMultiplier =
                    Mathf.Lerp(
                        minimumFlicker,
                        maximumFlicker,
                        noise
                    );
            }

            targetIntensity =
                baseIntensity *
                flickerMultiplier;
        }

        if (shouldBeActive)
        {
            targetLight.enabled = true;
        }

        targetLight.intensity =
            Mathf.MoveTowards(
                targetLight.intensity,
                targetIntensity,
                fadeSpeed *
                Time.deltaTime
            );

        if (!shouldBeActive &&
            targetLight.intensity <=
            0.001f)
        {
            targetLight.intensity = 0f;
            targetLight.enabled = false;
        }

        if (litVisual != null &&
            litVisual.activeSelf !=
            shouldBeActive)
        {
            litVisual.SetActive(
                shouldBeActive
            );
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Player"))
        {
            playersInside.Add(other);
        }
    }

    private void OnTriggerExit2D(
        Collider2D other
    )
    {
        if (other.CompareTag("Player"))
        {
            playersInside.Remove(other);
        }
    }

    private void OnDisable()
    {
        playersInside.Clear();

        if (targetLight != null)
        {
            targetLight.intensity = 0f;
            targetLight.enabled = false;
        }

        if (litVisual != null)
        {
            litVisual.SetActive(false);
        }
    }
}