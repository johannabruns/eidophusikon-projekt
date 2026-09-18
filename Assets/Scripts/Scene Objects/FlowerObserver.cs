using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FlowerObserver : NetworkBehaviour
{
    [Header("Flowers")]
    private BoolStateObject[] flowers;

    public NetworkVariable<bool> allFlowersActive =
        new NetworkVariable<bool>(false);

    public UnityEvent OnAllFlowersActive;

    [Header("Bees")]
    public StageObjectWrapper beesWrapper;
    public Animator beesAnimator;
    public string beesTriggerName = "Enter";

    private Coroutine beeAnimationRoutine;

    public override void OnNetworkSpawn()
    {
        allFlowersActive.OnValueChanged +=
            OnAllFlowersStateChanged;

        if (IsServer)
        {
            FindFlowers();

            if (beesWrapper != null)
            {
                beesWrapper.SetActive(
                    allFlowersActive.Value
                );
            }
        }

        if (allFlowersActive.Value)
        {
            StartBeeAnimation();
        }
    }

    public override void OnNetworkDespawn()
    {
        allFlowersActive.OnValueChanged -=
            OnAllFlowersStateChanged;

        if (beeAnimationRoutine != null)
        {
            StopCoroutine(beeAnimationRoutine);
            beeAnimationRoutine = null;
        }
    }

    private void FindFlowers()
    {
        flowers =
            GetComponentsInChildren
                <BoolStateObject>(true);
    }

    public void Check()
    {
        if (!IsServer ||
            allFlowersActive.Value)
        {
            return;
        }

        if (flowers == null ||
            flowers.Length == 0)
        {
            FindFlowers();
        }

        if (flowers == null ||
            flowers.Length == 0)
        {
            return;
        }

        foreach (BoolStateObject flower in flowers)
        {
            if (flower == null ||
                !flower.isActive.Value)
            {
                return;
            }
        }

        allFlowersActive.Value = true;

        OnAllFlowersActive?.Invoke();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance
                .CheckQuestCompletion();
        }
    }

    private void OnAllFlowersStateChanged(
        bool previous,
        bool current
    )
    {
        if (IsServer &&
            beesWrapper != null)
        {
            beesWrapper.SetActive(current);
        }

        if (current)
        {
            StartBeeAnimation();
        }
    }

    private void StartBeeAnimation()
    {
        if (beeAnimationRoutine != null)
        {
            StopCoroutine(beeAnimationRoutine);
        }

        beeAnimationRoutine =
            StartCoroutine(
                PlayBeeAnimationWhenVisible()
            );
    }

    private IEnumerator PlayBeeAnimationWhenVisible()
    {
        while (beesAnimator != null &&
               !beesAnimator.gameObject.activeInHierarchy)
        {
            yield return null;
        }

        if (beesAnimator != null &&
            !string.IsNullOrEmpty(beesTriggerName))
        {
            beesAnimator.ResetTrigger(
                beesTriggerName
            );

            beesAnimator.SetTrigger(
                beesTriggerName
            );
        }

        beeAnimationRoutine = null;
    }
}