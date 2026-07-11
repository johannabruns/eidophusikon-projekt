using Unity.Netcode;
using UnityEngine;

public class Lightning : NetworkBehaviour
{
    public GameObject lightningObj;
    public GameObject lightningImpactObj;
    public Animator lightningAnim;
    private SpriteRenderer lightningSprite;
    public AudioSource audioSource;
    public AudioClip soundEffect;

    public override void OnNetworkSpawn()
    {
        lightningSprite = lightningObj.GetComponent<SpriteRenderer>();
        lightningObj.SetActive(false);
        lightningImpactObj.SetActive(false);
    }

    public void Strike()
    {
        if(!IsServer) return; // Only the server should perform the raycast and trigger the strike

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 25f);
        if (hit.collider != null)
        {
            // World logic - runs once here, server-only, no RPC needed
            if (hit.collider.TryGetComponent(out ILightningTarget target))
            {
                target.OnStruck();
            }

            // Broadcast visuals/audio to everyone (data depends on server-side raycast)
            StrikeVisualsRpc(hit.distance, hit.point.y);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StrikeVisualsRpc(float distance, float impactY)
    {
        Debug.Log($"Lightning struck with distance {distance}");
        lightningSprite.size = new Vector2(lightningSprite.size.x, distance);
        lightningImpactObj.transform.position = new Vector3(transform.position.x, impactY, 0f);

        lightningAnim.SetTrigger("Strike");
        audioSource.PlayOneShot(soundEffect);

        lightningObj.SetActive(true);
        lightningImpactObj.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 25f);
    }
}