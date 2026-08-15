using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LeafPile : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public NetworkVariable<int> currentSpriteIndex = new(-1);

    public UnityEvent OnComplete;

    public override void OnNetworkSpawn()
    {
        currentSpriteIndex.OnValueChanged += OnSpriteIndexChanged;
        spriteRenderer.sprite = null;
    }
    public override void OnNetworkDespawn()
    {
        currentSpriteIndex.OnValueChanged -= OnSpriteIndexChanged;
    }

    private void OnSpriteIndexChanged(int previousValue, int newValue)
    {
        if (newValue < 0 || newValue >= sprites.Length) return;
        spriteRenderer.sprite = sprites[newValue];

        if(newValue == sprites.Length - 1)
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
