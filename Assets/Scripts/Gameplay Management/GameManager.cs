using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


[Serializable]
public class StageObjectGroup
{
    public string groupName;
    public StageObjectWrapper[] ojects;
}

public class GameManager : NetworkBehaviour
{
    private const int ExpectedPlayerCount = 2;

    [SerializeField] private StageObjectGroup[] objectGroups;
    public TheaterManager theaterManager;
    private Coroutine loadActCoroutine;

    private bool initialSetupDone = false;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    /// <summary>
    /// Ensure that the client has joined before the initial setup to avoid syncing issues.
    /// </summary>
    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < ExpectedPlayerCount) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        StartCoroutine(InitialActSetup());
    }


    /// <summary>
    /// Start the game by enabling the first act's objects.
    /// </summary>
    private IEnumerator InitialActSetup()
    {
        if (initialSetupDone) yield break;
        initialSetupDone = true;

        yield return null;
        EnableActObjects(1);
    }

    /// <summary>
    /// force the initial setup even if no client has joined (for debugging purposes).
    /// </summary>
    [Rpc(SendTo.Server)]
    public void ForceInitialActSetupRpc()
    {
        if (initialSetupDone) return; // guard against double-firing if a client does connect after
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        StartCoroutine(InitialActSetup());
    }


    [Rpc(SendTo.Server)]
    public void LoadActRpc(int actIndex)
    {
        Debug.Log($"LOADING ACT {actIndex}");

        if (loadActCoroutine != null)
        {
            StopCoroutine(loadActCoroutine);
        }
        loadActCoroutine = StartCoroutine(LoadAct(actIndex));
    }

    private IEnumerator LoadAct(int actIndex)
    {
        if(!IsServer) yield break;

        theaterManager.CloseCurtains();

        yield return new WaitForSeconds(3f);

        EnableActObjects(actIndex);

        yield return new WaitForSeconds(1f);

        theaterManager.OpenCurtains();
    }

    private void EnableActObjects(int actIndex)
    {
        if (!IsServer) return;

        for (int i = 0; i < objectGroups.Length; i++)
        {
            bool isActive = (i == actIndex - 1);
            foreach (StageObjectWrapper obj in objectGroups[i].ojects)
            {
                if (obj == null) continue;
                if (isActive && obj.spawnManually) continue;

                obj.SetActive(isActive);
            }
        }
    }
}
