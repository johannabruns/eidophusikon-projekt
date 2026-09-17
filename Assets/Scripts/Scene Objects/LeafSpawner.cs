using Unity.Netcode;
using UnityEngine;

public class LeafSpawner : NetworkBehaviour
{
    public GameObject leafPrefab;
    public Wind windScript;

    private bool done;

    private void OnEnable()
    {
        done = false;
    }

    private void Update()
    {
        if (!IsServer ||
            done)
        {
            return;
        }

        if (windScript == null)
        {
            windScript =
                FindFirstObjectByType<Wind>(
                    FindObjectsInactive.Include
                );
        }

        if (windScript == null ||
            !windScript.WindActive)
        {
            return;
        }

        SpawnLeaf();
        done = true;
    }

    private void SpawnLeaf()
    {
        if (!IsServer ||
            leafPrefab == null)
        {
            return;
        }

        GameObject spawnedLeaf =
            Instantiate(
                leafPrefab,
                transform.position,
                Quaternion.Euler(
                    0f,
                    0f,
                    Random.Range(
                        0f,
                        359f
                    )
                )
            );

        NetworkObject networkObject =
            spawnedLeaf
                .GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Destroy(spawnedLeaf);
            return;
        }

        networkObject.Spawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            0.2f
        );
    }
}