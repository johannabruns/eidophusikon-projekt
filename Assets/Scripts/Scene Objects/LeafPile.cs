using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LeafPile : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public NetworkVariable<int> currentSpriteIndex = new(0);

    public Wind windScript;
    public GameObject leafPrefab;
    public Transform leafSpawnPoint;
    private Coroutine destroyCoroutine;

    public AudioSource audioSource;
    public AudioClip onDestroyed;

    public bool IsComplete => currentSpriteIndex.Value >= sprites.Length - 1;
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
            OnComplete.Invoke();
            QuestManager.Instance.CheckQuestCompletion();
        }
    }

    [Rpc(SendTo.Server)]
    public void AddLeafRpc()
    {
        if (currentSpriteIndex.Value >= sprites.Length - 1)       
            return;

        currentSpriteIndex.Value++;
    }

    public IEnumerator DestroyPile()
    {
        if (destroyCoroutine != null) yield break;

        yield return new WaitForSeconds(1f);

        audioSource.PlayOneShot(onDestroyed);

        for (int i = 0; i < currentSpriteIndex.Value; i++)
        {
            GameObject leaf = Instantiate(leafPrefab, leafSpawnPoint.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 359f)));
            leaf.GetComponent<NetworkObject>().Spawn();
            //yield return new WaitForSeconds(0.2f);
        }

        currentSpriteIndex.Value = 0;
        destroyCoroutine = null;
    }

    private bool IsInProgress()
    {
        return currentSpriteIndex.Value > 0 && currentSpriteIndex.Value < sprites.Length - 1;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (windScript.WindActive && IsInProgress() && destroyCoroutine == null)
        {
            destroyCoroutine = StartCoroutine(DestroyPile());
        }
    }

}
