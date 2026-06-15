using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Collectible : MonoBehaviour
{
    public Collider2D coll;
    public AudioSource audioSource;
    public AudioClip clip;
    public UnityEvent onCollect;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        coll.isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spriteRenderer.enabled = false;
            audioSource.PlayOneShot(clip);
            onCollect.Invoke();
            Destroy(gameObject, clip.length);
        }
    }
}
