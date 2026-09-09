using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ConstellationManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<bool> isAligned = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [HideInInspector]
    public NetworkVariable<int> alignedStars = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public List<Star> stars;
    public NetworkVariable<int> currentStarIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Transform currentStarIndicator;

    public AxisInteractible axisInteractible;

    public UnityEvent OnConstellationAligned;

    public override void OnNetworkSpawn()
    {
        foreach (var star in stars)
        {
            star.reachedPosition.OnValueChanged += OnStarPositionChanged;
        }

        if (IsServer)
        {
            alignedStars.Value = stars.FindAll(s => s.reachedPosition.Value).Count;
            isAligned.Value = alignedStars.Value == stars.Count;
        }

        isAligned.OnValueChanged += OnConstellationStateChanged;
        currentStarIndex.OnValueChanged += OnActiveStarChanged;

        UpdateTargetStar(currentStarIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        foreach (var star in stars)
        {
            star.reachedPosition.OnValueChanged -= OnStarPositionChanged;
        }

        isAligned.OnValueChanged -= OnConstellationStateChanged;
        currentStarIndex.OnValueChanged -= OnActiveStarChanged;
    }

    private void OnStarPositionChanged(bool previous, bool current)
    {
        if (!IsServer) return;

        if (current)
        {
            alignedStars.Value++;
        }
        else
        {
            alignedStars.Value--;
        }

        isAligned.Value = alignedStars.Value == stars.Count;
    }

    private void OnConstellationStateChanged(bool previous, bool current)
    {
        if (current)
        {
            QuestManager.Instance.CheckQuestCompletion();
            OnConstellationAligned.Invoke();
        }
    }

    public void SwitchTargetStar()
    {
        SwitchTargetStarRpc();
    }

    [Rpc(SendTo.Server)]
    private void SwitchTargetStarRpc()
    {
        if (stars.Count == 0)
            return;

        currentStarIndex.Value = currentStarIndex.Value == stars.Count - 1 ? 0 : currentStarIndex.Value + 1;
    }

    private void UpdateTargetStar(int index)
    {
        if (stars[index].gameObject.TryGetComponent(out Movable target) && axisInteractible != null)
            axisInteractible.target = target;

        else if (axisInteractible == null)
            Debug.LogWarning("AxisInteractible is not assigned in ConstellationManager.");
    }

    private void OnActiveStarChanged(int previous, int current) => UpdateTargetStar(current);


    private void FixedUpdate()
    {
        if (stars.Count == 0)
            return;

        currentStarIndicator.position = stars[currentStarIndex.Value].transform.position;
    }
}
