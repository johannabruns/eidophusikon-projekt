using UnityEngine;

public class Carryable : SimpleInteractible
{
    public GameObject carryableObject;
    private PlayerInteraction script;

    public override void OnInteract()
    {
        script = player.GetComponent<PlayerInteraction>();

        if (script.IsCarryingItem) script.DropItem(carryableObject);
        else script.PickUpItem(carryableObject);
    }
}