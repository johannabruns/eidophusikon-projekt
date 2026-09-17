using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class CarryableHoverGlow : MonoBehaviour
{
    [Header("Hover Visuals")]
    public Light2D hoverLight;

    [Range(0f, 1f)]
    public float brightness = 0.25f;

    private Carryable carryable;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private bool pointerIsOver;
    private bool visualsAreActive;

    private void Awake()
    {
        carryable = GetComponent<Carryable>();
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        originalColors =
            new Color[spriteRenderers.Length];

        for (int i = 0;
             i < spriteRenderers.Length;
             i++)
        {
            originalColors[i] =
                spriteRenderers[i].color;
        }

        if (hoverLight == null)
        {
            hoverLight =
                GetComponentInChildren<Light2D>(true);
        }

        ApplyHoverVisuals(false);
    }

    private void Update()
    {
        bool itemIsAvailable =
            carryable != null &&
            !carryable.isCarried.Value &&
            !carryable.isSocketed.Value;

        bool shouldShowVisuals =
            pointerIsOver && itemIsAvailable;

        if (shouldShowVisuals !=
            visualsAreActive)
        {
            ApplyHoverVisuals(
                shouldShowVisuals
            );
        }
    }

    private void OnMouseEnter()
    {
        pointerIsOver = true;
    }

    private void OnMouseExit()
    {
        pointerIsOver = false;
    }

    private void OnDisable()
    {
        pointerIsOver = false;
        ApplyHoverVisuals(false);
    }

    private void ApplyHoverVisuals(bool active)
    {
        visualsAreActive = active;

        for (int i = 0;
             i < spriteRenderers.Length;
             i++)
        {
            Color originalColor =
                originalColors[i];

            Color brighterColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    originalColor.a
                );

            spriteRenderers[i].color =
                active
                    ? Color.Lerp(
                        originalColor,
                        brighterColor,
                        brightness
                    )
                    : originalColor;
        }

        if (hoverLight != null)
        {
            hoverLight.enabled = active;
        }
    }
}