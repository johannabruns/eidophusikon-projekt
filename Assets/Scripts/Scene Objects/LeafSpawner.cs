using Unity.Netcode;
using UnityEngine;

public class LeafSpawner : NetworkBehaviour
{

    public float windResistance;
    private bool done = false;

    public GameObject leafPrefab;
    public Wind windScript;

    public void SpawnLeaf()
    {
        if(!IsServer) return;  
        GameObject drop = Instantiate(leafPrefab, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 359f)));
        drop.GetComponent<NetworkObject>().Spawn();
    }

    private void Update()
    {
        if (!IsServer) return;

        if(!done)
        {
            if(windScript.currentIntensity.Value > windResistance)
            {
                SpawnLeaf();
                done = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
