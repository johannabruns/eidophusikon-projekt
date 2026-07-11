using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactible : NetworkBehaviour
{
    public Collider2D range;
    public GameObject UI;
    protected GameObject player;

    public override void OnNetworkSpawn()
    {
        // Ensure the range collider is set to trigger and disable the UI initially
        if (range == null)
            range = GetComponent<Collider2D>();

        range.isTrigger = true;
        if (UI != null) UI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;

            if(other.TryGetComponent(out PlayerInteraction playerInteraction))
            {
                playerInteraction.AddInteractible(this);
            }     
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            if (UI != null) UI.SetActive(false);

            if (other.TryGetComponent(out PlayerInteraction playerInteraction))
                playerInteraction.RemoveInteractible(this);
        }         
    }
}
