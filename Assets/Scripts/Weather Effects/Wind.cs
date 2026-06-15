using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Wind : NetworkBehaviour
{
    public enum WindDirection
    {
        LeftToRight,
        RightToLeft,
    }

    [Header("Dependencies")]
    public Animator animator;
    public AnimationClip windAnimation;
    public AnimationClip windFadeAnimation;
    public AudioSource audioSource;

    [Header("Configuration")]
    public float intensity = 1f;
    private float currentIntensity = 0f;
    public bool startOnAwake;
    [Space(1f)]
    public LayerMask ignoreLayers;

    private Rigidbody2D[] rigidbodies;
    private Coroutine soundFade;

    // Synced state
    private NetworkVariable<bool> windActive = new NetworkVariable<bool>(false);
    private NetworkVariable<WindDirection> direction = new NetworkVariable<WindDirection>(WindDirection.RightToLeft);

    public event Action<bool, bool> WindActiveChanged;

    public bool WindActive => windActive.Value;
    public WindDirection Direction => direction.Value;

    private void Awake()
    {
        if (IsServer && startOnAwake)
            StartWind();
    }

    public override void OnNetworkSpawn()
    {
        // Apply current state immediately (no callback fires for initial sync)
        animator.SetFloat("Direction", direction.Value == WindDirection.LeftToRight ? -1f : 1f);
        animator.SetBool("WindActive", windActive.Value);
        if (windActive.Value)
            FadeInSound();

        windActive.OnValueChanged += OnWindActiveChanged;
        direction.OnValueChanged += OnDirectionChanged;
    }

    public override void OnNetworkDespawn()
    {
        windActive.OnValueChanged -= OnWindActiveChanged;
        direction.OnValueChanged -= OnDirectionChanged;
    }

    // --- Public API (call from server-side logic, e.g. via Interactible's onPressServer) ---

    public void StartWind()
    {
        if (!IsServer || windActive.Value) return;
        windActive.Value = true;
        currentIntensity = intensity;
    }

    public void StopWind()
    {
        if (!IsServer || !windActive.Value) return;
        windActive.Value = false;
    }

    public void ToggleWind()
    {
        if (windActive.Value) StopWind();
        else StartWind();
    }

    public void SetDirection(WindDirection newDirection)
    {
        if (!IsServer) return;

        if (windActive.Value)
        {
            StartCoroutine(DirectionChangeTransition(newDirection));
            return;
        }

        direction.Value = newDirection;
    }

    private IEnumerator DirectionChangeTransition(WindDirection newDirection)
    {
        StopWind();
        yield return new WaitForSeconds(windFadeAnimation.length);
        direction.Value = newDirection;
        StartWind();
    }

    // --- Local reactions to synced state (run on every peer) ---

    private void OnWindActiveChanged(bool previous, bool current)
    {
        animator.SetBool("WindActive", current);
        if (current)
            FadeInSound();
        else
            FadeOutSound();

        WindActiveChanged?.Invoke(previous, current);
    }

    private void OnDirectionChanged(WindDirection previous, WindDirection current)
    {
        animator.SetFloat("Direction", current == WindDirection.LeftToRight ? -1f : 1f);
    }

    // --- Server-only physics ---

    private Rigidbody2D[] FindRigidBodies()
    {
        Rigidbody2D[] allRigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        return System.Array.FindAll(allRigidbodies, rb => (ignoreLayers.value & (1 << rb.gameObject.layer)) == 0);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (windActive.Value)
            ApplyWindForce();
        else if (currentIntensity > 0f)
            currentIntensity -= Time.deltaTime / windFadeAnimation.length;
    }

    private void ApplyWindForce()
    {
        rigidbodies ??= FindRigidBodies();
        Vector2 forceDirection = direction.Value == WindDirection.LeftToRight ? Vector2.right : Vector2.left;
        Vector2 force = forceDirection * currentIntensity;

        foreach (Rigidbody2D rb in rigidbodies)
            rb.AddForce(force);
    }

    private void FadeOutSound()
    {
        if (soundFade != null) StopCoroutine(soundFade);
        soundFade = StartCoroutine(AudioFader.FadeOut(audioSource, 2f));
    }

    private void FadeInSound()
    {
        if (soundFade != null) StopCoroutine(soundFade);
        soundFade = StartCoroutine(AudioFader.FadeIn(audioSource, 2f, 0.8f));
    }
}