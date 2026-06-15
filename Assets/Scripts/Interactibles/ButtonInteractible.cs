using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ButtonInteractible : SimpleInteractible
{
    public Animator animator;
    public UnityEvent onPress;
    public AudioSource audioSource;
    public AudioClip soundEffect;

    public override void OnInteract()
    {
        PressServerRpc();
    }

    // Client to server: "I pressed the button, Invoke callback"
    [Rpc(SendTo.Server)]
    private void PressServerRpc()
    {
        PressEveryoneRpc();
        onPress.Invoke();
    }

    // Server to clients: "The button was pressed, do the visuals and audio"
    [Rpc(SendTo.Everyone)]
    private void PressEveryoneRpc()
    {
        OnPressLocal();
    }

    private void OnPressLocal()
    {
        Debug.Log($"Button has been Interacted with");
        animator.SetTrigger("Activate");
        audioSource.PlayOneShot(soundEffect);
    }
}