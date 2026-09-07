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

    public Collider2D colldier;
    public CameraScript camScript;
    public PlatformEffector2D platformEffector;

    public override void OnNetworkSpawn()
    {
        //The host player is assigned to the machine room and may not enter the stage
        if(IsHost) SetEntranceValues(StageState.Locked);

        //Open the entrance for client to enter the stage
        else if (IsClient) SetEntranceValues(StageState.OneWayStage);
    }


    public void SetStageLockState(StageState state)
    {
        if(!IsServer) return;
        SetEntranceValues(state);
    }

    private void SetEntranceValues(StageState state)
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
