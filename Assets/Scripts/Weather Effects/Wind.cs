using System.Collections;
using System.Linq;
using UnityEngine;

public class Wind : MonoBehaviour
{
    public enum Direction
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
    public Direction direction = Direction.RightToLeft;
    public LayerMask ignoreLayers;

    private Rigidbody2D[] rigidbodies;

    public bool WindActive { get; private set; }

    private Coroutine soundFade;

    private void Awake()
    {
        SetDirection(direction);

        if (startOnAwake)
        StartWind();
    }

    public void StartWind()
    {
        if (WindActive)
            return;

        animator.SetBool("WindActive", true);
        FadeInSound();

        currentIntensity = intensity;

        WindActive = true;
    }

    public void StopWind()
    {
        if (!WindActive)
            return;
        
        animator.SetBool("WindActive", false);
        FadeOutSound();

        WindActive = false;
    }

    public void ToggleWind()
    {
        if (WindActive) StopWind();
        else StartWind();
    }

    public void SetDirection(Direction direction)
    {
        this.direction = direction;

        //Play the transition animation if the wind is active, otherwise just set the direction
        if (WindActive)
        {
            StartCoroutine(DirectionChangeTransition());
            return;
        }

        animator.SetFloat("Direction", direction == Direction.LeftToRight ? -1f : 1f);
    }

    private IEnumerator DirectionChangeTransition()
    {
        StopWind();

        yield return new WaitForSeconds(windFadeAnimation.length);
        animator.SetFloat("Direction", direction == Direction.LeftToRight ? -1f : 1f);

        StartWind();
    }

    private Rigidbody2D[] FindRigidBodies()
    {
        Rigidbody2D[] rigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        Rigidbody2D[] allRigidbodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        Rigidbody2D[] filteredRigidbodies = System.Array.FindAll(allRigidbodies, rb => (ignoreLayers.value & (1 << rb.gameObject.layer)) == 0);

        System.Array.ForEach(filteredRigidbodies, rb => Debug.Log($"Rigidbody found attached to {rb.gameObject.name}"));

        return filteredRigidbodies;
    }

    private void FixedUpdate()
    {
        if (WindActive)
            ApplyWindForce();

        if (!WindActive && currentIntensity > 0f)
        {
            currentIntensity -= Time.deltaTime / windFadeAnimation.length;
        }
    }

    private void  ApplyWindForce()
    { 
        rigidbodies ??= FindRigidBodies();

        Vector2 forceDirection = direction == Direction.LeftToRight ? Vector2.right : Vector2.left;
        Vector2 force = forceDirection * currentIntensity;
        foreach (Rigidbody2D rb in rigidbodies)
        {
            rb.AddForce(force);
        }
    }

    private void FadeOutSound()
    {
        if (soundFade != null)
        {
            StopCoroutine(soundFade);
            soundFade = null;
        }

        soundFade = StartCoroutine(AudioFader.FadeOut(audioSource, 2f));
    }

    private void FadeInSound()
    {
        if (soundFade != null)
        {
            StopCoroutine(soundFade);
            soundFade = null;
        }
        soundFade = StartCoroutine(AudioFader.FadeIn(audioSource, 2f, 0.8f));
    }



}
