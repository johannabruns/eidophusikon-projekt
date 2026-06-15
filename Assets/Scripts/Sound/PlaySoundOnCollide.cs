using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnCollide : MonoBehaviour
{
    public AudioClip collisionSound;
    public AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(WaitOnLoad());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }

    private IEnumerator WaitOnLoad()
    {
        audioSource.mute = true;
        yield return new WaitForSeconds(1f);
        audioSource.mute = false;
    }
}