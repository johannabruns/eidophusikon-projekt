using Unity.Netcode;
using UnityEngine;

public class RainCollector : BoolStateObject, IRainTarget
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public void OnRainHit()
    {
        if(!isActive.Value)        
            SetActive(true);       
    }

    public void CollectRain()
    {
        if(isActive.Value)
        {
            SetActive(false);
        }
    }
}
