using UnityEngine;
using UnityEngine.Events;

public abstract class Usable : Carryable
{
    [SerializeField] protected Animator anim;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip onUseSound;

    public abstract void OnUse();
}
