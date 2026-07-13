using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConstellationManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<bool> isAligned = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [HideInInspector]
    public NetworkVariable<int> alignedStars = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public List<Star> stars;
    //private Star currentstar => stars[currentStarIndex.Value];

    public NetworkVariable<int> currentStarIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Transform currentStarIndicator;

    public AxisInteractible axisInteractible;

    public override void OnNetworkSpawn()
    {
        foreach (var star in stars)
        {
            star.reachedPosition.OnValueChanged += OnStarPositionChanged;
        }

        alignedStars.Value = stars.FindAll(s => s.reachedPosition.Value).Count;
        isAligned.Value = alignedStars.Value == stars.Count;

        isAligned.OnValueChanged += OnConstellationAligned;

        if(stars[currentStarIndex.Value].gameObject.TryGetComponent(out Movable target)) 
        {
            axisInteractible.target = target;
        }
    }

    public override void OnNetworkDespawn()
    {
        foreach (var star in stars)
        {
            star.reachedPosition.OnValueChanged -= OnStarPositionChanged;
        }

        isAligned.OnValueChanged -= OnConstellationAligned;
    }

    private void OnStarPositionChanged(bool previous, bool current)
    {
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

    private void OnConstellationAligned(bool previous, bool current)
    {
        if (current)
        {
            Debug.Log("Constellation aligned!");
        }

        else if (previous && !current)
        {
            Debug.Log("Constellation misaligned!");
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

        if (stars[currentStarIndex.Value].gameObject.TryGetComponent(out Movable target))
        {
            axisInteractible.target = target;
        }
    }

    private void FixedUpdate()
    {
        currentStarIndicator.position = stars[currentStarIndex.Value].transform.position;
    }
}
