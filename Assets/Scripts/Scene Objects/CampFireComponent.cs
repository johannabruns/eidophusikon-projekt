using Unity.Netcode;
using UnityEngine;

public class CampFireComponent : Usable
{
    public enum CampFireComponentType
    {
        Wood,
        Stone
    }

    [SerializeField] private CampFireComponentType campFireComponentType;

    public override void OnUse()
    {
        if (IsObjectInRange(out CampFire campFire))
        {
            switch (campFireComponentType)
            {
                case CampFireComponentType.Wood:
                    campFire.AddWoodRpc();
                    break;
                case CampFireComponentType.Stone:
                    campFire.AddStoneRpc();
                    break;
            }
            audioSource.PlayOneShot(onUseSound);
            RequestObjectDestructionRpc();
        }
    }
}
