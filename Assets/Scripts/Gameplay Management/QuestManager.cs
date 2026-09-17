using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public PlayerSpawner playerSpawner;

    [Header("Vorhang")]
    public StopMotionRope curtainRope;

    [Header("Act Transition")]
    [Min(0f)]
    public float curtainCloseTransitionDelay = 3f;

    [Header("Regiebuch")]
    public QuestOverlayManager overlayManager;

    [Header("Quests")]
    public List<Quest> quests;

    [Header("Finale")]
    [Min(0f)]
    public float finaleTeleportDelay = 0.35f;

    [Min(0f)]
    public float finalAnimationDuration = 60f;

    [Header("Debug")]
    public bool startWithOnePlayerForDebug;
    public bool enableDebugActSkipping;

    [HideInInspector]
    public NetworkVariable<int> currentAct =
        new NetworkVariable<int>(0);

    public NetworkVariable<bool>
        curtainControlEnabled =
            new NetworkVariable<bool>(false);

    private Quest currentQuest;
    private Coroutine actTransition;
    private Coroutine finaleSequence;

    private bool initialFlowStarted;
    private bool curtainWasOpenedForCurrentQuest;
    private bool questCompletionInProgress;
    private bool gameFinished;
    private bool finaleStarted;

    public static Action<int> OnQuestComplete;
    public static Action OnFinaleComplete;

    public bool CurtainControlEnabled =>
        curtainControlEnabled.Value;

    public override void OnNetworkSpawn()
    {
        if (Instance != null &&
            Instance != this)
        {
            return;
        }

        Instance = this;

        if (IsServer &&
            overlayManager != null)
        {
            overlayManager.OnAllPlayersReady +=
                HandleAllPlayersReady;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer &&
            overlayManager != null)
        {
            overlayManager.OnAllPlayersReady -=
                HandleAllPlayersReady;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;

        TryStartInitialFlow();
        TryHandleDebugSkip();

        if (currentQuest == null ||
            questCompletionInProgress ||
            gameFinished ||
            !curtainControlEnabled.Value)
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

        if (IsFinalQuest())
        {
            if (AreCurrentQuestGoalsComplete())
            {
                StartFinale();
            }

            return;
        }

        if (IsCurtainClosed())
        {
            CheckQuestCompletion();
        }
    }

    private void TryHandleDebugSkip()
    {
        if (!enableDebugActSkipping ||
            Keyboard.current == null ||
            !Keyboard.current.f8Key
                .wasPressedThisFrame)
        {
            return;
        }

        DebugSkipCurrentAct();
    }

    [ContextMenu("DEBUG Skip Current Act")]
    public void DebugSkipCurrentAct()
    {
        if (!IsServer ||
            !enableDebugActSkipping ||
            currentQuest == null ||
            questCompletionInProgress ||
            gameFinished ||
            finaleStarted ||
            actTransition != null)
        {
            return;
        }

        if (IsFinalQuest())
        {
            StartFinale();
            return;
        }

        curtainControlEnabled.Value =
            false;

        SetCurtainClosedForDebug();
        CompleteCurrentQuest();
    }

    private void SetCurtainClosedForDebug()
    {
        if (curtainRope != null &&
            curtainRope.ropeFrames != null &&
            curtainRope.ropeFrames.Length > 0)
        {
            int closedFrame =
                curtainRope
                    .longestStateOpensCurtain
                    ? 0
                    : curtainRope
                        .ropeFrames
                        .Length - 1;

            curtainRope.currentFrame.Value =
                closedFrame;
        }

        if (theaterManager != null)
        {
            theaterManager.CloseCurtains();
        }
    }

    private void TryStartInitialFlow()
    {
        if (initialFlowStarted ||
            currentAct.Value != 0 ||
            NetworkManager.Singleton == null)
        {
            return;
        }

        int requiredPlayers =
            startWithOnePlayerForDebug
                ? 1
                : 2;

        if (NetworkManager.Singleton
                .ConnectedClientsIds
                .Count <
            requiredPlayers)
        {
            return;
        }

        PrepareFirstQuest();
    }

    private void PrepareFirstQuest()
    {
        if (!IsServer ||
            initialFlowStarted ||
            quests == null ||
            quests.Count == 0)
        {
            return;
        }

        initialFlowStarted = true;

        if (theaterManager != null)
        {
            theaterManager.LongApplause();
        }

        ActivateQuest(1);
        StartActBriefing(1);
    }

    private void StartActBriefing(
        int actIndex
    )
    {
        curtainControlEnabled.Value =
            false;

        curtainWasOpenedForCurrentQuest =
            false;

        questCompletionInProgress =
            true;

        if (overlayManager != null)
        {
            overlayManager.ShowActPage(
                actIndex
            );
        }
        else
        {
            ReleaseAct(actIndex);
        }
    }

    private void HandleAllPlayersReady(
        int actIndex
    )
    {
        if (!IsServer ||
            actIndex != currentAct.Value)
        {
            return;
        }

        ReleaseAct(actIndex);
    }

    private void ReleaseAct(
        int actIndex
    )
    {
        if (!IsServer ||
            actIndex != currentAct.Value)
        {
            return;
        }

        curtainWasOpenedForCurrentQuest =
            false;

        questCompletionInProgress =
            false;

        curtainControlEnabled.Value =
            true;
    }

    public void CheckQuestCompletion()
    {
        if (!IsServer ||
            currentQuest == null ||
            questCompletionInProgress ||
            gameFinished ||
            !curtainControlEnabled.Value ||
            !curtainWasOpenedForCurrentQuest)
        {
            return;
        }

        if (!AreCurrentQuestGoalsComplete())
        {
            return;
        }

        if (IsFinalQuest())
        {
            StartFinale();
            return;
        }

        if (!IsCurtainClosed())
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

        if (!HasNextQuest())
        {
            StartFinale();
            return;
        }

        questCompletionInProgress = true;
        curtainControlEnabled.Value = false;

        OnQuestComplete?.Invoke(
            currentAct.Value
        );

        float questDelay =
            Mathf.Max(
                0f,
                currentQuest.nextQuestDelay
            );

        actTransition =
            StartCoroutine(
                QuestTransition(
                    questDelay
                )
            );
    }

    private IEnumerator QuestTransition(
        float questDelay
    )
    {
        if (theaterManager != null)
        {
            theaterManager.ShortApplause();
        }

        float transitionDelay =
            Mathf.Max(
                curtainCloseTransitionDelay,
                questDelay
            );

        if (transitionDelay > 0f)
        {
            yield return new WaitForSeconds(
                transitionDelay
            );
        }

        int nextAct =
            currentAct.Value + 1;

        ActivateQuest(nextAct);
        StartActBriefing(nextAct);

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

    private void StartFinale()
    {
        if (!IsServer ||
            finaleStarted ||
            currentQuest == null)
        {
            return;
        }

        finaleStarted = true;
        gameFinished = true;
        questCompletionInProgress = true;

        curtainControlEnabled.Value =
            false;

        finaleSequence =
            StartCoroutine(
                PlayFinaleSequence()
            );
    }

    private IEnumerator PlayFinaleSequence()
    {
        if (theaterManager != null)
        {
            theaterManager.OpenCurtains();
        }

        if (playerSpawner != null)
        {
            playerSpawner
                .TeleportMechanicToFinale();
        }

        yield return new WaitForSeconds(
            finaleTeleportDelay
        );

        OnQuestComplete?.Invoke(
            currentAct.Value
        );

        yield return new WaitForSeconds(
            finalAnimationDuration
        );

        if (theaterManager != null)
        {
            theaterManager.LongApplause();
        }

        OnFinaleComplete?.Invoke();

        finaleSequence = null;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    public void StartFirstQuestRpc()
    {
        if (currentAct.Value != 0)
            return;

        PrepareFirstQuest();
    }

    public bool HasNextQuest()
    {
        return
            currentAct.Value <
            quests.Count;
    }

    private bool IsFinalQuest()
    {
        return
            quests != null &&
            quests.Count > 0 &&
            currentAct.Value ==
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