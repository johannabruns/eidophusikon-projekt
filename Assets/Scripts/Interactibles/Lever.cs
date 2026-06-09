using UnityEngine;
using UnityEngine.Events;

public class Lever : SimpleInteractible
{
    public bool isEnabled = false;
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip clip;

    [Header("Interaction Events")]
    public UnityEvent onLeverEnabled;
    public UnityEvent onLeverDisabled;

    private void Awake()
    {
        animator.SetBool("enabled", isEnabled);
    }

    public override void OnInteract()
    {
        isEnabled = !isEnabled;
        animator.SetBool("enabled", isEnabled);
        audioSource.PlayOneShot(clip);

        if (isEnabled)      
            onLeverEnabled.Invoke();     
        else
            onLeverDisabled.Invoke();
    }
}
