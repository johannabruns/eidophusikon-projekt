using Unity.Netcode;
using UnityEngine;

public class TheaterManager : NetworkBehaviour
{
    public Animator animator;


    public void OpenCurtains()
    {
        SetCurtainStateRpc(true);
    }

    public void CloseCurtains()
    {
        SetCurtainStateRpc(false);
    }

    [Rpc(SendTo.Server)]
    private void SetCurtainStateRpc(bool state)
    {
        animator.SetBool("IsOpen", state);
    }

}
