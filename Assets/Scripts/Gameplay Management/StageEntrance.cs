using System.Data.SqlTypes;
using Unity.Netcode;
using UnityEngine;

public class StageEntrance : NetworkBehaviour
{
    public enum StageState
    {
        Locked,
        OneWayStage,
        OneWayHallway,
        Open
    }

    [SerializeField]
    private NetworkVariable<StageState> stageLockState = new(StageState.Locked);

    public Collider2D colldier;
    public CameraScript camScript;
    public PlatformEffector2D platformEffector;

    public override void OnNetworkSpawn()
    {
        stageLockState.OnValueChanged += OnStageLockStateChanged;
        SetStageLockState(stageLockState.Value);
    }

    public override void OnNetworkDespawn()
    {
        stageLockState.OnValueChanged -= OnStageLockStateChanged;
    }

    [Rpc(SendTo.Server)]
    public void SetStageLockStateRpc(StageState state)
    {
        stageLockState.Value = state;
    }

    private void OnStageLockStateChanged(StageState previous, StageState current)
    {
        SetStageLockState(current);
    }

    private void SetStageLockState(StageState state)
    {
        switch (state)
        {
            case StageState.Locked:
                platformEffector.surfaceArc = 360f;
                platformEffector.rotationalOffset = 0f;
                break;
            case StageState.OneWayStage:
                platformEffector.surfaceArc = 180f;
                platformEffector.rotationalOffset = -90f;
                break;
            case StageState.OneWayHallway:
                platformEffector.surfaceArc = 180f;
                platformEffector.rotationalOffset = 90;
                break;
            case StageState.Open:
                platformEffector.surfaceArc = 0f;
                platformEffector.rotationalOffset = 0f;
                break;
        }
    }

}
