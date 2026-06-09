using System.Collections;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public GameObject lightningObj;
    public GameObject lightningImpactObj;
    public Animator lightningAnim;
    private SpriteRenderer lightningSprite;

    public AudioSource audioSource;
    public AudioClip soundEffect;

    private Ray ray;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightningSprite = lightningObj.GetComponent<SpriteRenderer>();
        lightningObj.SetActive(false);
        lightningImpactObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Strike()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 25f);

        if (hit.collider != null)
        {
            //configure lightning graphics according to hit distance
            lightningSprite.size = new Vector2(lightningSprite.size.x, hit.distance);
            lightningImpactObj.transform.position = new Vector3(transform.position.x, hit.point.y, 0f);
            lightningAnim.SetTrigger("Strike");

            audioSource.PlayOneShot(soundEffect);

            //trigger logic if hit is lightning target
            if (hit.collider.TryGetComponent(out ILightningTarget target))        
                target.OnStruck();
            
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 25f);
    }
}
