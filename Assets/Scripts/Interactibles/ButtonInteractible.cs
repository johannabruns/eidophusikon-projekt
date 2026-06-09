using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class ButtonInteractible : SimpleInteractible
{
    public Animator animator;
    public UnityEvent onPress;

    public AudioSource audioSource;
    public AudioClip soundEffect;

    public override void OnInteract()
    {
        Press();
    }

    public void Press()
    {
        onPress.Invoke();
        animator.SetTrigger("Activate");
        audioSource.PlayOneShot(soundEffect);
    }
}
