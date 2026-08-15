using Unity.Netcode;
using UnityEngine;

public class Leaf : Usable
{
    public override void OnUse()
    {
        if(IsObjectInRange(out LeafPile pile))
        {
            audioSource.PlayOneShot(onUseSound);
            pile.AddLeafRpc();
            RequestObjectDestructionRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestObjectDestructionRpc()
    {
        NetworkObject.Despawn();
    }
}
