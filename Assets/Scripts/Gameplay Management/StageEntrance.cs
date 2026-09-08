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

    //configurable for debugging purposes, but should normally be set to Locked
    public StageState hostStageState;

    public Collider2D colldier;
    public CameraScript camScript;
    public PlatformEffector2D platformEffector;

    public override void OnNetworkSpawn()
    {
        //The host player is assigned to the machine room and may not enter the stage
        if(IsServer) SetEntranceValues(hostStageState);

        //Open the entrance for client to enter the stage
        else if (!IsServer) SetEntranceValues(StageState.OneWayStage);
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void SetStageLockStateRpc(Player player, StageState state)
    {   
        if (player == Player.StagePlayer && IsServer)
            return;

        if (player == Player.MachineRoomPlayer && !IsServer)
            return;

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
