using UnityEngine;

public class Cage : MonoBehaviour
{
    public bool IsOpen {  get; private set; }
    public bool startOpen = false;

    [Header("Dependencies")]
    public Animator animator;
    public BoxCollider2D coll;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private void Start()
    {
        if (startOpen)
            Open();
    }

    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        animator.SetBool("IsOpen", true);
        coll.enabled = false;
        audioSource.PlayOneShot(openSound);
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        animator.SetBool("IsOpen", false);
        coll.enabled = true;
        audioSource.PlayOneShot(closeSound);
    }
}
