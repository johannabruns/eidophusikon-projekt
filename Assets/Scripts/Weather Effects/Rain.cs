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
    public AudioSource audioSource;
    private Coroutine soundFade;

    public List<Transform> spawnPoints;
    private List<RainDropSpawner> spawners = new List<RainDropSpawner>();

    //private float cooldownTimer = 0f;

    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        isActive.OnValueChanged += OnIsActiveChanged;

        foreach(Transform spawnPoint in spawnPoints)
        {
            float initialCooldown = GetRandomSpawnInterval();
            spawners.Add(new RainDropSpawner(spawnPoint, initialCooldown));
        }
          
        
    }

    public override void OnNetworkDespawn()
    {
        isActive.OnValueChanged -= OnIsActiveChanged;
    }

    private void Update()
    {
        if (!IsServer)
            return;


        if (isActive.Value)
        {
            /*
            if(cooldownTimer <= 0f)
            {
                foreach(Transform spawnPoint in spawnPoints)
                {
                    Instantiate(rainDropPrefab, spawnPoint.position, Quaternion.identity);
                }
                cooldownTimer = 0.35f;
            }
            else
            {
                cooldownTimer -= Time.deltaTime;
            }
            */

            foreach (RainDropSpawner spawner in spawners)
            {
                if (spawner.cooldown <= 0f)
                {
                    Instantiate(rainDropPrefab, spawner.spawnPoint.position, Quaternion.identity);
                    spawner.cooldown = GetRandomSpawnInterval();
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
        isActive.Value = !isActive.Value;
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
            FadeInSound();
        }
        else
        {
            FadeOutSound();
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
        return Random.Range(0.1f, 0.5f);
    }
}
