using Unity.Netcode;
using UnityEngine;

public class Key : Usable
{
    public override void OnUse()
    {
        if(IsObjectInRange(out Birdcage birdcage))
        {
            birdcage.UnlockRpc(true);
            RequestObjectDestructionRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestObjectDestructionRpc()
    {
        NetworkObject.Despawn();
    }

}
