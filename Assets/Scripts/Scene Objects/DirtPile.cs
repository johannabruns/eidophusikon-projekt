using Unity.Netcode;
using UnityEngine;

public class DirtPile : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public NetworkVariable<int> currentSpriteIndex = new(0);

    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    public override void OnNetworkSpawn()
    {
        spriteRenderer.sprite = sprites[currentSpriteIndex.Value];
        currentSpriteIndex.OnValueChanged += OnSpriteIndexChanged;
    }
    public override void OnNetworkDespawn()
    {
        currentSpriteIndex.OnValueChanged -= OnSpriteIndexChanged;
    }

    private void OnSpriteIndexChanged(int previousValue, int newValue)
    {
        if (newValue < 0 || newValue >= sprites.Length) return;
        spriteRenderer.sprite = sprites[newValue];
    }

    [Rpc(SendTo.Server)]
    public void DigRpc()
    {
        if (currentSpriteIndex.Value >= sprites.Length - 1)
        {
            GameObject key = Instantiate(keyPrefab, keySpawnPoint.position, Quaternion.identity);
            key.GetComponent<NetworkObject>().Spawn(true);
            NetworkObject.Despawn();
        }
        else
        {
            currentSpriteIndex.Value++;
        }
    }
}
