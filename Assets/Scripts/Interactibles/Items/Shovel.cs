using UnityEngine;

public class Shovel : Usable
{
    public override void OnUse()
    {
        anim.SetTrigger("Use");

        if (IsObjectInRange(out DirtPile pile))
        {
            audioSource.PlayOneShot(onUseSound);
            pile.DigRpc();
        }
    }
}


