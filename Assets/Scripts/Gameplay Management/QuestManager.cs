using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class QuestManager : NetworkBehaviour
{
    public static QuestManager Instance
    {
        get;
        private set;
    }

    [Header("Manager")]
    public TheaterManager theaterManager;
    public GameManager gameManager;

    [Header("Vorhang")]
    public StopMotionRope curtainRope;

    [Header("Quests")]
    public List<Quest> quests;

    [HideInInspector]
    public NetworkVariable<int> currentAct =
        new NetworkVariable<int>(0);

    private Quest currentQuest;
    private Coroutine actTransition;

    private bool curtainWasOpenedForCurrentQuest;
    private bool questCompletionInProgress;
    private bool gameFinished;

    public static Action<int> OnQuestComplete;

    public override void OnNetworkSpawn()
    {
        if (Instance != null &&
            Instance != this)
        {
            return;
        }

        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!IsServer ||
            currentQuest == null ||
            questCompletionInProgress ||
            gameFinished)
        {
            return;
        }

        if (!curtainWasOpenedForCurrentQuest)
        {
            if (IsCurtainOpen())
            {
                curtainWasOpenedForCurrentQuest =
                    true;
            }

            return;
        }

        if (IsCurtainClosed())
        {
            CheckQuestCompletion();
        }
    }

    public void CheckQuestCompletion()
    {
        if (!IsServer ||
            currentQuest == null ||
            questCompletionInProgress ||
            gameFinished ||
            !curtainWasOpenedForCurrentQuest ||
            !IsCurtainClosed())
        {
            return;
        }

        if (!AreCurrentQuestGoalsComplete())
        {
            return;
        }

        CompleteCurrentQuest();
    }

    private bool AreCurrentQuestGoalsComplete()
    {
        if (theaterManager == null ||
            theaterManager.lights == null ||
            currentQuest == null)
        {
            return false;
        }

        bool correctTimeOfDay =
            currentQuest.timeOfDay ==
            theaterManager
                .lights
                .currentTimeOfDay
                .Value;

        return
            correctTimeOfDay &&
            currentQuest.IsComplete();
    }

    private void CompleteCurrentQuest()
    {
        if (questCompletionInProgress)
            return;

        questCompletionInProgress = true;

        OnQuestComplete?.Invoke(
            currentAct.Value
        );

        if (!HasNextQuest())
        {
            gameFinished = true;

            if (theaterManager != null)
            {
                theaterManager.LongApplause();
            }

            return;
        }

        float delay =
            Mathf.Max(
                0f,
                currentQuest.nextQuestDelay
            );

        actTransition =
            StartCoroutine(
                QuestTransition(delay)
            );
    }

    private IEnumerator QuestTransition(
        float delay
    )
    {
        if (theaterManager != null)
        {
            theaterManager.ShortApplause();
        }

        yield return new WaitForSeconds(
            delay
        );

        int nextAct =
            currentAct.Value + 1;

        ActivateQuest(nextAct);

        questCompletionInProgress = false;
        actTransition = null;
    }

    private void ActivateQuest(
        int actIndex
    )
    {
        if (!IsServer ||
            actIndex < 1 ||
            actIndex > quests.Count)
        {
            return;
        }

        currentAct.Value = actIndex;
        currentQuest =
            quests[actIndex - 1];

        curtainWasOpenedForCurrentQuest =
            false;

        gameManager.LoadAct(actIndex);
    }

    [Rpc(SendTo.Server)]
    public void StartFirstQuestRpc()
    {
        if (currentAct.Value != 0 ||
            quests == null ||
            quests.Count == 0)
        {
            return;
        }

        if (theaterManager != null)
        {
            theaterManager.LongApplause();
        }

        ActivateQuest(1);
    }

    public bool HasNextQuest()
    {
        return
            currentAct.Value <
            quests.Count;
    }

    private bool IsCurtainOpen()
    {
        if (curtainRope == null)
            return false;

        return
            curtainRope
                .longestStateOpensCurtain
                ? curtainRope.IsLongest
                : curtainRope.IsShortest;
    }

    private bool IsCurtainClosed()
    {
        if (curtainRope == null)
            return false;

        return
            curtainRope
                .longestStateOpensCurtain
                ? curtainRope.IsShortest
                : curtainRope.IsLongest;
    }
}