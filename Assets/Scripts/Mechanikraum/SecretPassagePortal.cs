using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class SecretPassagePortal : MonoBehaviour
{
    public enum ActivationMode
    {
        Automatic,
        Interaction
    }

    [Header("Portal")]
    [SerializeField] private ActivationMode activationMode;
    [SerializeField] private Transform destination;

    [Header("Nur für den manuellen Ausgang")]
    [SerializeField] private InputActionReference exitAction;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Übergang")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.2f;
    [SerializeField, Min(0f)] private float invisibleDuration = 0.05f;

    private readonly HashSet<Collider2D> hostCollidersInRange = new();

    private NetworkObject currentPlayer;
    private bool transitionRunning;

    private void Start()
    {
        SetPromptVisible(false);
    }

    private void Update()
    {
        if (activationMode != ActivationMode.Interaction)
            return;

        if (transitionRunning || currentPlayer == null)
            return;

        if (exitAction == null || exitAction.action == null)
            return;

        if (exitAction.action.WasPressedThisFrame())
        {
            BeginTransition(currentPlayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        NetworkObject player = GetLocalHostPlayer(other);

        if (player == null)
            return;

        hostCollidersInRange.Add(other);
        currentPlayer = player;

        if (activationMode == ActivationMode.Automatic)
        {
            BeginTransition(player);
        }
        else
        {
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        hostCollidersInRange.Remove(other);

        if (hostCollidersInRange.Count > 0)
            return;

        currentPlayer = null;
        SetPromptVisible(false);
    }

    private NetworkObject GetLocalHostPlayer(Collider2D other)
    {
        if (NetworkManager.Singleton == null)
            return null;

        // Diese Passage darf ausschließlich auf dem Host ausgeführt werden.
        if (!NetworkManager.Singleton.IsHost)
            return null;

        NetworkObject player = other.GetComponentInParent<NetworkObject>();

        if (player == null)
            return null;

        if (!player.CompareTag("Player"))
            return null;

        // Nur der lokal kontrollierte Host-Spieler darf passieren.
        if (!player.IsOwner)
            return null;

        if (player.OwnerClientId != NetworkManager.ServerClientId)
            return null;

        // Verhindert, dass andere NetworkObjects versehentlich erkannt werden.
        if (player.GetComponent<PlayerMovement>() == null)
            return null;

        return player;
    }

    private void BeginTransition(NetworkObject player)
    {
        if (transitionRunning || player == null || destination == null)
            return;

        transitionRunning = true;

        hostCollidersInRange.Clear();
        currentPlayer = null;
        SetPromptVisible(false);

        StartCoroutine(TeleportRoutine(player));
    }

    private IEnumerator TeleportRoutine(NetworkObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        Rigidbody2D rigidBody = player.GetComponent<Rigidbody2D>();
        NetworkTransform networkTransform = player.GetComponent<NetworkTransform>();

        SpriteRenderer[] renderers =
            player.GetComponentsInChildren<SpriteRenderer>(true);

        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }

        bool movementWasEnabled = movement != null && movement.enabled;

        if (movement != null)
            movement.enabled = false;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
        }

        // Eido ausblenden
        yield return FadeSprites(renderers, originalColors, 1f, 0f);

        Vector3 targetPosition = destination.position;

        // Der NetworkTransform von Eido ist owner-authoritative.
        // Da diese Routine nur beim Host-Eigentümer läuft, darf er teleportieren.
        if (networkTransform != null)
        {
            networkTransform.Teleport(
                targetPosition,
                player.transform.rotation,
                player.transform.localScale
            );
        }
        else
        {
            player.transform.position = targetPosition;
        }

        if (rigidBody != null)
        {
            rigidBody.position = targetPosition;
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
        }

        Physics2D.SyncTransforms();

        if (invisibleDuration > 0f)
            yield return new WaitForSeconds(invisibleDuration);

        // Eido am Ziel wieder einblenden
        yield return FadeSprites(renderers, originalColors, 0f, 1f);

        if (movement != null)
            movement.enabled = movementWasEnabled;

        transitionRunning = false;
    }

    private IEnumerator FadeSprites(
        SpriteRenderer[] renderers,
        Color[] originalColors,
        float startVisibility,
        float targetVisibility)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            float visibility =
                Mathf.Lerp(startVisibility, targetVisibility, progress);

            ApplyVisibility(renderers, originalColors, visibility);

            yield return null;
        }

        ApplyVisibility(renderers, originalColors, targetVisibility);
    }

    private void ApplyVisibility(
        SpriteRenderer[] renderers,
        Color[] originalColors,
        float visibility)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color color = originalColors[i];
            color.a = originalColors[i].a * visibility;
            renderers[i].color = color;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(visible);
    }
}