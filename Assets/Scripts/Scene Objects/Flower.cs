using UnityEngine;

public class Flower : BoolStateObject, IRainTarget
{
    public void OnRainHit()
    {
        SetActive(true);
    }
}
