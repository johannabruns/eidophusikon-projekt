using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ConstellationManager : NetworkBehaviour
{
    [HideInInspector]
    public NetworkVariable<bool> isAligned =
        new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [HideInInspector]
    public NetworkVariable<int> alignedStars =
        new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public List<Star> stars;

    public NetworkVariable<int> currentStarIndex =
        new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public Transform currentStarIndicator;
    public AxisInteractible axisInteractible;
    public UnityEvent OnConstellationAligned;

    [Header("Target Confirmation")]
    [Min(0.01f)]
    public float releaseConfirmationDelay = 0.2f;

    [Min(0.0001f)]
    public float movementEpsilon = 0.001f;

    private Vector3 lastActiveStarPosition;
    private float stationaryTime;
    private bool activeStarHasMoved;
    private bool trackingInitialized;

    public override void OnNetworkSpawn()
    {
        foreach (Star star in stars)
        {
            if (star != null)
            {
                star.reachedPosition.OnValueChanged +=
                    OnStarPositionChanged;
            }
        }

        if (IsServer)
        {
            RefreshAlignedStarCount();
            isAligned.Value = false;
        }

        isAligned.OnValueChanged +=
            OnConstellationStateChanged;

        currentStarIndex.OnValueChanged +=
            OnActiveStarChanged;

        UpdateTargetStar(
            currentStarIndex.Value
        );

        ResetActiveStarTracking();
        ApplyWheelLock(isAligned.Value);
    }

    public override void OnNetworkDespawn()
    {
        foreach (Star star in stars)
        {
            if (star != null)
            {
                star.reachedPosition.OnValueChanged -=
                    OnStarPositionChanged;
            }
        }

        isAligned.OnValueChanged -=
            OnConstellationStateChanged;

        currentStarIndex.OnValueChanged -=
            OnActiveStarChanged;
    }

    private void Update()
    {
        if (!IsServer ||
            isAligned.Value ||
            stars.Count == 0)
        {
            return;
        }

        int index =
            currentStarIndex.Value;

        if (index < 0 ||
            index >= stars.Count ||
            stars[index] == null)
        {
            return;
        }

        Star activeStar =
            stars[index];

        Vector3 currentPosition =
            activeStar.transform.position;

        if (!trackingInitialized)
        {
            lastActiveStarPosition =
                currentPosition;

            trackingInitialized = true;
            return;
        }

        float movement =
            Vector3.Distance(
                currentPosition,
                lastActiveStarPosition
            );

        lastActiveStarPosition =
            currentPosition;

        if (movement > movementEpsilon)
        {
            activeStarHasMoved = true;
            stationaryTime = 0f;
            return;
        }

        if (!activeStarHasMoved ||
            !activeStar.reachedPosition.Value)
        {
            stationaryTime = 0f;
            return;
        }

        stationaryTime += Time.deltaTime;

        if (stationaryTime >=
            releaseConfirmationDelay)
        {
            ConfirmActiveStar();
        }
    }

    private void ConfirmActiveStar()
    {
        int index =
            currentStarIndex.Value;

        if (index < 0 ||
            index >= stars.Count ||
            stars[index] == null)
        {
            return;
        }

        Star activeStar =
            stars[index];

        if (activeStar.targetPosition != null)
        {
            activeStar.transform.position =
                activeStar.targetPosition.position;

            Physics2D.SyncTransforms();
        }

        RefreshAlignedStarCount();

        if (alignedStars.Value ==
            stars.Count)
        {
            isAligned.Value = true;
            return;
        }

        SelectNextUnalignedStar();
    }

    private void OnStarPositionChanged(
        bool previous,
        bool current
    )
    {
        if (!IsServer)
            return;

        RefreshAlignedStarCount();

        if (!current &&
            isAligned.Value)
        {
            isAligned.Value = false;
        }
    }

    private void RefreshAlignedStarCount()
    {
        int reachedStars = 0;

        foreach (Star star in stars)
        {
            if (star != null &&
                star.reachedPosition.Value)
            {
                reachedStars++;
            }
        }

        alignedStars.Value =
            reachedStars;
    }

    private void SelectNextUnalignedStar()
    {
        if (stars.Count == 0)
            return;

        int activeIndex =
            currentStarIndex.Value;

        for (
            int offset = 1;
            offset <= stars.Count;
            offset++
        )
        {
            int nextIndex =
                (activeIndex + offset) %
                stars.Count;

            Star nextStar =
                stars[nextIndex];

            if (nextStar != null &&
                !nextStar.reachedPosition.Value)
            {
                currentStarIndex.Value =
                    nextIndex;

                return;
            }
        }
    }

    private void OnConstellationStateChanged(
        bool previous,
        bool current
    )
    {
        ApplyWheelLock(current);

        if (current)
        {
            QuestManager.Instance
                .CheckQuestCompletion();

            OnConstellationAligned.Invoke();
        }
    }

    private void ApplyWheelLock(
        bool locked
    )
    {
        if (axisInteractible != null)
        {
            axisInteractible.enabled =
                !locked;

            if (axisInteractible.range != null)
            {
                axisInteractible.range.enabled =
                    !locked;
            }

            if (axisInteractible.rayTargetCollider != null)
            {
                axisInteractible
                    .rayTargetCollider.enabled =
                    !locked;
            }

            if (locked &&
                axisInteractible.UI != null)
            {
                axisInteractible.UI.SetActive(
                    false
                );
            }
        }

        if (currentStarIndicator != null)
        {
            currentStarIndicator.gameObject
                .SetActive(!locked);
        }
    }

    public void SwitchTargetStar()
    {
        SwitchTargetStarRpc();
    }

    [Rpc(SendTo.Server)]
    private void SwitchTargetStarRpc()
    {
        if (!isAligned.Value)
        {
            SelectNextUnalignedStar();
        }
    }

    private void UpdateTargetStar(
        int index
    )
    {
        if (axisInteractible == null ||
            index < 0 ||
            index >= stars.Count ||
            stars[index] == null)
        {
            return;
        }

        Movable target =
            stars[index]
                .GetComponent<Movable>();

        if (target != null)
        {
            axisInteractible.target =
                target;
        }
    }

    private void OnActiveStarChanged(
        int previous,
        int current
    )
    {
        UpdateTargetStar(current);
        ResetActiveStarTracking();
    }

    private void ResetActiveStarTracking()
    {
        stationaryTime = 0f;
        activeStarHasMoved = false;
        trackingInitialized = false;
    }

    private void FixedUpdate()
    {
        if (stars.Count == 0 ||
            currentStarIndicator == null ||
            isAligned.Value)
        {
            return;
        }

        int index =
            currentStarIndex.Value;

        if (index < 0 ||
            index >= stars.Count ||
            stars[index] == null)
        {
            return;
        }

        currentStarIndicator.position =
            stars[index].transform.position;
    }
}