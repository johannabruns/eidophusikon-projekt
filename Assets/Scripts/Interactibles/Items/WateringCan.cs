using Unity.Netcode;
using UnityEngine;

public class WateringCan : Usable, IRainTarget
{
    NetworkVariable<bool> isFull = new NetworkVariable<bool>();
    public Rain rain;
    private float rainDuration;

    public void OnRainHit()
    {
        SetWateringCanStateRpc(true);
    }

    [Rpc(SendTo.Server)]
    private void SetWateringCanStateRpc(bool isFull)
    {
        this.isFull.Value = isFull;
        Debug.Log($"Watering can state set to: {isFull}");
    }

    public override void OnUse()
    {
        anim.SetTrigger("Use");
        if (isFull.Value)
        {
            SetWateringCanStateRpc(false);
            rainDuration = 2f;
            audioSource.PlayOneShot(onUseSound);
        }
    }

    private void FixedUpdate()
    {
        if (rainDuration > 0f && !rain.isRaining.Value)
        {
            rain.ToggleRainRpc();
        }
        else if (rainDuration > 0f && rain.isRaining.Value)
        {
            rainDuration -= Time.fixedDeltaTime;
        }
        else if (rainDuration <= 0f && rain.isRaining.Value)
        {
            rain.ToggleRainRpc();
        }
    }
}
