using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FlowerObserver : NetworkBehaviour
{
    [Header("Flowers")]
    private BoolStateObject[] flowers;

    public NetworkVariable<bool>
        allFlowersActive =
            new NetworkVariable<bool>(false);

    public UnityEvent OnAllFlowersActive;

    [Header("Bees")]
    public GameObject beesObject;
    public Animator beesAnimator;
    public string beesTriggerName = "Enter";

    public override void OnNetworkSpawn()
    {
        allFlowersActive.OnValueChanged +=
            OnAllFlowersStateChanged;

        ApplyBeeState(
            allFlowersActive.Value
        );

        if (IsServer)
        {
            FindFlowers();
        }
    }

    public override void OnNetworkDespawn()
    {
        allFlowersActive.OnValueChanged -=
            OnAllFlowersStateChanged;
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

        foreach (
            BoolStateObject flower
            in flowers
        )
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
        ApplyBeeState(current);
    }

    private void ApplyBeeState(
        bool active
    )
    {
        if (beesObject == null)
            return;

        beesObject.SetActive(active);

        if (!active ||
            beesAnimator == null ||
            string.IsNullOrEmpty(
                beesTriggerName
            ))
        {
            return;
        }

        beesAnimator.ResetTrigger(
            beesTriggerName
        );

        beesAnimator.SetTrigger(
            beesTriggerName
        );
    }
}