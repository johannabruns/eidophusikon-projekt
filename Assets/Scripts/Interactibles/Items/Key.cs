using Unity.Netcode;
using UnityEngine;

public class Key : Usable
{
    public override void OnUse()
    {
        if(IsObjectInRange(out Birdcage birdcage))
        {
            //audioSource.PlayOneShot(onUseSound);
            birdcage.UnlockRpc(true);
            RequestObjectDestructionRpc();
        }
    }

}
