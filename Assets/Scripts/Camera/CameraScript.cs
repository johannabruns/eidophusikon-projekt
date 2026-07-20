using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public CinemachineCamera playerFollowCam;
    public CinemachineCamera stageCam;
    public CinemachineCamera machineRoomCam;
    public CameraMode currentMode = CameraMode.PlayerFollow;
    public Transform playerTransform;

    public void SetCameraMode(CameraMode newMode)
    {
        currentMode = newMode;
        switch (currentMode)
        {
            case CameraMode.StageView:
                EnterStageMode();
                break;
            case CameraMode.MachineRoomView:
                EnterMachineRoomMode();
                break;
            case CameraMode.PlayerFollow:
                EnterPlayerFollowMode(playerTransform);
                break;
        }
    }

    public void EnterPlayerFollowMode(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
        Debug.Log("Entering Player Follow Mode");
        playerFollowCam.Target.TrackingTarget = playerTransform;
        playerFollowCam.Priority = 1;
        stageCam.Priority = 0;
        machineRoomCam.Priority = 0;
    }

    public void EnterPlayerFollowMode()
    {
        Debug.Log("Entering Player Follow Mode");
        playerFollowCam.Target.TrackingTarget = playerTransform;
        playerFollowCam.Priority = 1;
        stageCam.Priority = 0;
        machineRoomCam.Priority = 0;
    }

    public void EnterStageMode()
    {
        Debug.Log("Entering Stage Mode");
        playerFollowCam.Priority = 0;
        stageCam.Priority = 1;
        machineRoomCam.Priority = 0;
    }

    public void EnterMachineRoomMode()
    {
        Debug.Log("Entering Machine Room Mode");
        playerFollowCam.Priority = 0;
        stageCam.Priority = 0;
        machineRoomCam.Priority = 1;
    }
}
