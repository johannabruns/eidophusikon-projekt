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
            RequestObjectDestructionRpc(onUseSound.length + 0.5f);
        }
    }
}
