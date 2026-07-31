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

    private NetworkVariable<StageState> stageState = new(StageState.OneWayStage);
    public Collider2D colldier;
    public CameraScript camScript;
    public PlatformEffector2D platformEffector;

    public override void OnNetworkSpawn()
    {
        stageState.OnValueChanged += OnIsLockedChanged;
        SetStageState(stageState.Value);
    }

    public override void OnNetworkDespawn()
    {
        stageState.OnValueChanged -= OnIsLockedChanged;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }

    [Rpc(SendTo.Server)]
    public void SetStageLockServerRpc(StageState state)
    {
        this.stageState.Value = state;
    }

    private void OnIsLockedChanged(StageState previous, StageState current)
    {
        SetStageState(current);
    }

    private void SetStageState(StageState state)
    {
        switch (state)
        {
            case StageState.Locked:
                platformEffector.surfaceArc = 0f;
                platformEffector.rotationalOffset = 360f;
                break;
            case StageState.OneWayStage:
                platformEffector.surfaceArc = 180f;
                platformEffector.rotationalOffset = -90f;
                break;
            case StageState.OneWayHallway:
                platformEffector.surfaceArc = 90f;
                platformEffector.rotationalOffset = 180f;
                break;
            case StageState.Open:
                platformEffector.surfaceArc = 0f;
                platformEffector.rotationalOffset = 0f;
                break;
        }
    }

}
