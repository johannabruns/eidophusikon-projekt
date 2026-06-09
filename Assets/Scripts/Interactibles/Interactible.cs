using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactible : MonoBehaviour
{
    public Collider2D range;
    public GameObject UI;
    protected GameObject player;

    private void Awake()
    {
        if (range == null)
            range = GetComponent<Collider2D>();

        range.isTrigger = true;
    }

    private void Start()
    {
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
