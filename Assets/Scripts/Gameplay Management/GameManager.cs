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
    [SerializeField] private StageObjectGroup[] objectGroups;
    public TheaterManager theaterManager;
    private Coroutine loadActCoroutine;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(InitialActSetup());
    }

    private IEnumerator InitialActSetup()
    {
        yield return null; // let every in-scene object finish spawning this frame
        Debug.Log("Initial act setup running");
        EnableActObjects(1); //spawn the first act objects
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

    public IEnumerator LoadAct(int actIndex)
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
                Debug.Log($"Currently targeting {obj.gameObject.name}, isActive: {isActive}");
                if (isActive && obj.spawnManually) continue;
                obj.SetActive(isActive);
            }
        }
    }
}
