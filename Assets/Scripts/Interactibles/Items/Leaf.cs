using Unity.Netcode;
using UnityEngine;

public class Leaf : Usable
{
    private NetworkVariable<bool> hasBeenUsed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);   

    public override void OnUse()
    {
        if(IsObjectInRange(out LeafPile pile))
        {
            if (hasBeenUsed.Value) return;

            hasBeenUsed.Value = true;
            audioSource.PlayOneShot(onUseSound);
            pile.AddLeafRpc();
            RequestObjectDestructionRpc(onUseSound.length + 0.5f);
        }
    }
}
