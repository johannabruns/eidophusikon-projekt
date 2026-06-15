using UnityEngine;
using UnityEngine.Events;

public class Generator : MonoBehaviour, ILightningTarget
{
    private bool isActive;
    public Animator animator;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    public AudioSource audioSource;
    public AudioClip soundEffect;

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        animator.SetBool("IsActive", true);
        onActivate.Invoke();
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;
        animator.SetBool("IsActive", false);
        onDeactivate.Invoke();
    }

    public void OnStruck()
    {
        audioSource.PlayOneShot(soundEffect);

        if (isActive)
            Deactivate();
        else
            Activate();
    }
}
