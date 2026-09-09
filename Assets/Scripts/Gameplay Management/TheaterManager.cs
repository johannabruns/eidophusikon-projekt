using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This class manages gameplay elements related to the theater setting, such as curtain animations and audience reactions.
/// </summary>
public class TheaterManager : NetworkBehaviour
{
    public StageLightManager lights;

    public Animator curtainAnim;
    public StageEntrance entrance;

    public AudioSource audioSource;

    public AudioClip longApplause;
    public AudioClip shortApplause;
    public AudioClip finalApplause;

    public void ShortApplause()     
    {
        audioSource.PlayOneShot(shortApplause);
    }
    public void LongApplause()
    {
        audioSource.PlayOneShot(longApplause);
    }

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
        curtainAnim.SetBool("IsOpen", state);
    }
}
