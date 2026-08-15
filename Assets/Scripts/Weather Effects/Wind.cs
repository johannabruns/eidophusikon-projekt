using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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
    public NetworkVariable<float> targetIntensity = new(1f);
    public NetworkVariable<float> currentIntensity = new(0f);
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
        animator.SetBool("WindActive", windActive.Value);
        UpdateWindAnimation(currentIntensity.Value, direction.Value);
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

    [Rpc(SendTo.Server)]
    public void StartWindRpc()
    {
        StartWind();
    }

    private void StartWind()
    {
        if (!IsServer || windActive.Value) return;
        windActive.Value = true;
        SetIntensity(targetIntensity.Value);
    }

    [Rpc(SendTo.Server)]
    public void StopWindRpc()
    {
        StopWind();
    }

    private void StopWind()
    {
        if (!IsServer || !windActive.Value) return;
        windActive.Value = false;
    }

    [Rpc(SendTo.Server)]
    public void ToggleWindRpc()
    {
        ToggleWind();
    }

    private void ToggleWind()
    {
        if (windActive.Value) StopWind();
        else StartWind();
    }

    [Rpc(SendTo.Server)]
    public void SetDirectionRpc(WindDirection newDirection)
    {
        SetDirection(newDirection);
    }

    private void SetDirection(WindDirection newDirection)
    {
        if (!IsServer) return;

        if (windActive.Value)
        {
            StartCoroutine(DirectionChangeTransition(newDirection));
            return;
        }

        direction.Value = newDirection;
    }

    [Rpc(SendTo.Server)]
    public void ToggleDirectionRpc()
    {
        ToggleDirection();
    }

    private void ToggleDirection()
    {
        if (direction.Value == WindDirection.LeftToRight)
            SetDirection(WindDirection.RightToLeft);
        else
            SetDirection(WindDirection.LeftToRight);
    }

    private IEnumerator DirectionChangeTransition(WindDirection newDirection)
    {
        StopWind();
        yield return new WaitForSeconds(windFadeAnimation.length);
        direction.Value = newDirection;
        StartWind();
    }

    [Rpc(SendTo.Server)]
    public void SetIntensityRpc(float newIntensity)
    {
        SetIntensity(newIntensity);
    }

    private void SetIntensity(float newIntensity)
    {
        if (!IsServer) return;

        switch(newIntensity)
        {
            case < 1f:
                newIntensity = 1f;
                break;
            case > 10f:
                newIntensity = 10f;
                break;
        }

        if (windActive.Value)
        currentIntensity.Value = newIntensity;
       
        targetIntensity.Value = newIntensity;

        UpdateWindAnimation(newIntensity, direction.Value);

        Debug.Log($"Wind intensity set to {newIntensity}");
    }

    // --- Local reactions to synced state (run on every peer) ---

    private void OnWindActiveChanged(bool previous, bool current)
    {
        animator.SetBool("WindActive", current);
        if (current)
        {
            UpdateWindAnimation(currentIntensity.Value, direction.Value);
            FadeInSound();
        }
        else
            FadeOutSound();

        WindActiveChanged?.Invoke(previous, current);
    }

    private void OnDirectionChanged(WindDirection previous, WindDirection current)
    {
        UpdateWindAnimation(currentIntensity.Value, current);
    }

    private void UpdateWindAnimation(float speed, WindDirection direction)
    {
        animator.SetFloat("Speed", direction == WindDirection.LeftToRight ? -speed : speed);
    }

    // --- Server-only physics ---

    private Rigidbody2D[] FindRigidBodies()
    {
        Rigidbody2D[] allRigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        Rigidbody2D[] filteredRbs = Array.FindAll(allRigidbodies, rb => (ignoreLayers.value & (1 << rb.gameObject.layer)) == 0);

        //DEBUG
        string rigidbodyNames = string.Join(", ", Array.ConvertAll(filteredRbs, rb => rb.gameObject.name));
        Debug.Log($"Found {filteredRbs.Length} rigidbodies affected by wind: {rigidbodyNames}");

        return filteredRbs;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (windActive.Value)
            ApplyWindForce();
        else if (currentIntensity.Value > 0f)
            currentIntensity.Value -= Time.deltaTime / windFadeAnimation.length;
    }
    private void ApplyWindForce()
    {
        rigidbodies ??= FindRigidBodies();
        Vector2 forceDirection = direction.Value == WindDirection.LeftToRight ? Vector2.right : Vector2.left;
        Vector2 force = forceDirection * currentIntensity.Value;

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