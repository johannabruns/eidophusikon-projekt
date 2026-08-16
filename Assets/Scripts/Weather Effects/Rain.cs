using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class Rain : NetworkBehaviour
{
    private class RainDropSpawner
    {
        public Transform spawnPoint;
        public float cooldown;

        public RainDropSpawner(Transform spawnPoint, float cooldown)
        {
            this.spawnPoint = spawnPoint;
            this.cooldown = cooldown;
        }
    }

    public GameObject rainDropPrefab;
    public float maxSpawnInterval = 0.5f;
    public bool constantInverval = false;
    public AudioSource audioSource;
    private Coroutine soundFade;

    public List<Transform> spawnPoints;
    private List<RainDropSpawner> spawners = new List<RainDropSpawner>();

    public NetworkVariable<bool> isRaining = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        isRaining.OnValueChanged += OnIsActiveChanged;

        foreach(Transform spawnPoint in spawnPoints)
        {
            float initialCooldown = constantInverval ? maxSpawnInterval : GetRandomSpawnInterval();
            spawners.Add(new RainDropSpawner(spawnPoint, initialCooldown));
        }
    }

    public override void OnNetworkDespawn()
    {
        isRaining.OnValueChanged -= OnIsActiveChanged;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;


        if (isRaining.Value)
        {
            foreach (RainDropSpawner spawner in spawners)
            {
                if (spawner.cooldown <= 0f)
                {
                    GameObject drop = Instantiate(rainDropPrefab, spawner.spawnPoint.position, Quaternion.identity);
                    drop.GetComponent<NetworkObject>().Spawn(true);

                    spawner.cooldown = constantInverval ? maxSpawnInterval : GetRandomSpawnInterval();
                }
                else
                {
                    spawner.cooldown -= Time.deltaTime;
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void ToggleRainRpc()
    {
        isRaining.Value = !isRaining.Value;
    }

    private void OnDrawGizmos()
    {
        foreach(Transform spawnPoint in spawnPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPoint.position, 0.1f);
        }
    }

    private void OnIsActiveChanged(bool previous, bool current)
    {
        if (current)
        {
            if (audioSource != null) FadeInSound();
        }
        else
        {
            if (audioSource != null) FadeOutSound();
        }
    }

    private void FadeOutSound()
    {
        if (soundFade != null) StopCoroutine(soundFade);
        soundFade = StartCoroutine(AudioFader.FadeOut(audioSource, 2f));
    }

    private void FadeInSound()
    {
        if (soundFade != null) StopCoroutine(soundFade);
        soundFade = StartCoroutine(AudioFader.FadeIn(audioSource, 2f));
    }

    private float GetRandomSpawnInterval()
    {
        return Random.Range(0.1f, maxSpawnInterval);
    }
}
