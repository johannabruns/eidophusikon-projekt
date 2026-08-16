using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class WateringCan : Usable
{
    NetworkVariable<bool> isFull = new NetworkVariable<bool>();

    public AudioClip refillSound;
    public SpriteRenderer spriteRenderer;
    public Sprite emptySprite;
    public Sprite fullSprite;
   

    public Rain waterStream;
    private float rainDuration = 0f;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isFull.OnValueChanged += OnWateringCanStateChanged;
        SetSprite(isFull.Value);
    }


    [Rpc(SendTo.Server)]
    private void SetWateringCanStateRpc(bool isFull)
    {
        this.isFull.Value = isFull;
    }

    public override void OnUse()
    {
        //Watering
        if (isFull.Value)
        {
            anim.SetTrigger("Use");
            SetWateringCanStateRpc(false);
            rainDuration = 1f;
            audioSource.PlayOneShot(onUseSound);
        }
        else
        {
            //Refilling
            if(IsObjectInRange(out RainCollector rainCollector))
            {
                if (!rainCollector.isActive.Value)
                {
                    anim.SetTrigger("Use");
                }
                else
                {
                    rainCollector.CollectRain();
                    SetWateringCanStateRpc(true);
                    audioSource.PlayOneShot(refillSound);
                    return;
                }
            }
            anim.SetTrigger("Use");
        }
    }

    private void OnWateringCanStateChanged(bool previousValue, bool newValue)
    {
        SetSprite(newValue);
    }

    private void SetSprite(bool isFull)
    {
        spriteRenderer.sprite = isFull ? fullSprite : emptySprite; 
    }

    private void FixedUpdate()
    {
        if (rainDuration > 0f && !waterStream.isRaining.Value)
        {
            waterStream.ToggleRainRpc();
        }
        else if (rainDuration > 0f && waterStream.isRaining.Value)
        {
            rainDuration -= Time.fixedDeltaTime;
        }
        else if (rainDuration <= 0f && waterStream.isRaining.Value)
        {
            waterStream.ToggleRainRpc();
        }
    }
}
