using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : NetworkBehaviour
{
    public static QuestManager Instance { get; private set; }

    public TheaterManager theaterManager;
    public GameManager gameManager;

    [HideInInspector]
    public NetworkVariable<int> currentAct = new NetworkVariable<int>(0);
    public List<Quest> quests;
    private Quest currentQuest;

    private Coroutine actTransition;
    public static Action<int> OnQuestComplete;

    public override void OnNetworkSpawn()
    {
        if (Instance != null && Instance != this)
            return;
       
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CheckQuestCompletion()
    {
        bool isComplete = true;

        //check if the correct time of day is set for the current quest
        if (currentQuest.timeOfDay != theaterManager.lights.currentTimeOfDay.Value)
            isComplete = false;

        //check if all requirements for the current quest are complete
        if (!currentQuest.IsComplete())
            isComplete = false;

        //TODO: Check if curtains are closed?

        if (isComplete)
        {
            OnQuestComplete.Invoke(currentAct.Value);
            StartNextQuest();
        }
          
    }

    private void StartNextQuest()
    {
        if (!IsServer) return;

        //special case if this is the final quest
        if (!HasNextQuest())
        {
            theaterManager.LongApplause();
            return;
        }

        float delay = 0f;
        if (currentQuest != null)
            delay = currentQuest.nextQuestDelay;

        currentAct.Value++;
        currentQuest = quests[currentAct.Value - 1];

        actTransition = StartCoroutine(QuestTransition(currentAct.Value, delay));
    }

    private IEnumerator QuestTransition(int index, float delay)
    {
        if (actTransition != null) yield break;

        //this block only executes if this is NOT the first quest
        if (currentAct.Value > 1)
        {
            theaterManager.ShortApplause();
            yield return new WaitForSeconds(delay);

            theaterManager.CloseCurtains();
            yield return new WaitForSeconds(3f);
        }

        gameManager.LoadAct(index);
        theaterManager.OpenCurtains();

        actTransition = null;
    }

    //Special case for the first quest, since it is not triggered by a quest completion but by the player entering the stage for the first time
    [Rpc(SendTo.Server)]
    public void StartFirstQuestRpc()
    {
        //confirm this is indeed the first quest
        if (currentAct.Value != 0) return;

        theaterManager.LongApplause();
        StartNextQuest();
    }

    public bool HasNextQuest()
    {
        return currentAct.Value < quests.Count;
    }  
}
