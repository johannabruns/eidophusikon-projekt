using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LeafPile : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public NetworkVariable<int> currentSpriteIndex = new(0);

    public UnityEvent OnComplete;

    public override void OnNetworkSpawn()
    {
        currentSpriteIndex.OnValueChanged += OnSpriteIndexChanged;
        SetSprite(currentSpriteIndex.Value);
    }
    public override void OnNetworkDespawn()
    {
        currentSpriteIndex.OnValueChanged -= OnSpriteIndexChanged;
    }

    private void OnSpriteIndexChanged(int previousValue, int newValue)
    {
        SetSprite(newValue);
    }

    private void SetSprite(int index)
    {
        if (index < 0 || index >= sprites.Length) return;
        spriteRenderer.sprite = sprites[index];

        if (index == sprites.Length - 1)
        {
            OnComplete?.Invoke();
        }
    }

    [Rpc(SendTo.Server)]
    public void AddLeafRpc()
    {
        if (currentSpriteIndex.Value >= sprites.Length - 1)       
            return;

        currentSpriteIndex.Value++;
    }
}
