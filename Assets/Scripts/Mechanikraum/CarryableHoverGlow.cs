using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class CarryableHoverGlow : MonoBehaviour
{
    [Header("Hover Visuals")]
    public Light2D hoverLight;

    [Range(0f, 1f)]
    public float brightness = 0.25f;

    private Carryable carryable;
    private Collider2D[] hoverColliders;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    private bool visualsAreActive;

    private void Awake()
    {
        carryable = GetComponent<Carryable>();

        hoverColliders =
            GetComponentsInChildren<Collider2D>(true);

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
        bool pointerIsOver =
            IsPointerOverCarryable();

        bool itemIsAvailable =
            carryable != null &&
            carryable.IsSpawned &&
            !carryable.isCarried.Value &&
            !carryable.isSocketed.Value;

        bool shouldShowVisuals =
            pointerIsOver &&
            itemIsAvailable;

        if (shouldShowVisuals !=
            visualsAreActive)
        {
            ApplyHoverVisuals(
                shouldShowVisuals
            );
        }
    }

    private bool IsPointerOverCarryable()
    {
        if (Mouse.current == null ||
            Camera.main == null)
        {
            return false;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            Camera.main.ScreenPointToRay(
                mousePosition
            );

        RaycastHit2D[] hits =
            Physics2D.GetRayIntersectionAll(
                ray,
                100f
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            foreach (
                Collider2D hoverCollider
                in hoverColliders
            )
            {
                if (hoverCollider != null &&
                    hit.collider ==
                    hoverCollider)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnDisable()
    {
        ApplyHoverVisuals(false);
    }

    private void ApplyHoverVisuals(bool active)
    {
        visualsAreActive = active;

        if (spriteRenderers != null &&
            originalColors != null)
        {
            for (int i = 0;
                 i < spriteRenderers.Length;
                 i++)
            {
                if (spriteRenderers[i] == null)
                    continue;

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
        }

        if (hoverLight != null)
        {
            hoverLight.enabled = active;
        }
    }
}