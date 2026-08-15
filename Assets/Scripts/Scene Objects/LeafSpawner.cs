using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        GameObject drop = Instantiate(leafPrefab, transform.position, Quaternion.identity);
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
