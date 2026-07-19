using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Birdcage : NetworkBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip unlockSound;

    public UnityEvent onUnlocked;

    public NetworkVariable<bool> isOpen = new(false);

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += OnIsOpenChanged;
    }

    public override void OnNetworkDespawn()
    {
        isOpen.OnValueChanged -= OnIsOpenChanged;
    }

    [Rpc(SendTo.Server)]
    public void UnlockRpc(bool newState)
    {
        if(isOpen.Value != newState)
        {
            isOpen.Value = newState;
            if(newState) onUnlocked.Invoke();          
        }
    }

    private void OnIsOpenChanged(bool previous, bool current)
    {
        if (current)
        {
            audioSource.PlayOneShot(unlockSound);
            animator.SetTrigger("Open");
        }
    }
}
