using UnityEngine;

public class Shovel : Usable
{
    public override void OnUse()
    {
        anim.SetTrigger("Use");
        audioSource.PlayOneShot(onUseSound);
    }

}
