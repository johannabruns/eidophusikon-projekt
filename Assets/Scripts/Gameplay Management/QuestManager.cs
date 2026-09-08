using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    public static QuestManager Instance { get; private set; }

    public TheaterManager theaterManager;
    public StageLightManager stageLightManager;
    public GameManager gameManager;

    public NetworkVariable<int> currentAct = new NetworkVariable<int>(0);
    public List<Quest> quests;
    private Quest currentQuest;

    private Coroutine actTransition;

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    public void CheckQuestCompletion()
    {
        if (!IsServer) return;

        bool isComplete = true;

        //check if the correct time of day is set for the current quest
        if (currentQuest.timeOfDay != stageLightManager.currentTimeOfDay.Value)
            isComplete = false;

        //check if all requirements for the current quest are complete
        if (!currentQuest.IsComplete())
            isComplete = false;

        //TODO: Check if curtains are closed?

        Debug.Log($"Checking for {currentAct.Value}: TimeOfDay should be {currentQuest.timeOfDay} and is: {stageLightManager.currentTimeOfDay.Value}" +
            $" | Quest requirements completed: {currentQuest.IsComplete()}");

        if (isComplete && currentAct.Value < quests.Count) NextQuest();
    }

    private void NextQuest()
    {
        if (!IsServer) return;

        //return if there is no next quest
        if (currentAct.Value >= quests.Count) return;

        currentAct.Value++;
        currentQuest = quests[currentAct.Value - 1];
        actTransition = StartCoroutine(QuestTransition(currentAct.Value));
    }

    private IEnumerator QuestTransition(int index)
    {
        if (actTransition != null) yield break;

        //don't close the curtains for the first quest, since the curtains are already closed at the start of the game
        if (currentAct.Value > 1)
        {
            theaterManager.CloseCurtains();
            yield return new WaitForSeconds(3f);
        }

        gameManager.LoadAct(index);

        yield return new WaitForSeconds(1f);

        theaterManager.OpenCurtains();

        actTransition = null;
    }

    //Special case for the first quest, since it is not triggered by a quest completion but by the player entering the stage for the first time
    [Rpc(SendTo.Server)]
    public void StartFirstQuestRpc()
    {
        //make sure the game has not started yet
        if (currentAct.Value != 0) return;

        NextQuest();
    }
}
