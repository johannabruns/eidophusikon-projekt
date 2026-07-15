using UnityEngine;

public class Flower : BoolStateObject, IRainTarget
{
    public void OnRainHit()
    {
        if(!IsServer) return;
        SetActive(true);
    }
}
