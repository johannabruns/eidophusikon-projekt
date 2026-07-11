using Unity.Netcode;
using UnityEngine;

public class Carryable : SimpleInteractible
{
    public GameObject carryableObject;
    private PlayerInteraction script;
    public NetworkVariable<bool> isCarried = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnInteract()
    {
        script = player.GetComponent<PlayerInteraction>();

        if (script.IsCarryingItem) script.DropItem(carryableObject);
        else script.PickUpItem(carryableObject);
    }
}