using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Lever : SimpleInteractible
{
    public NetworkVariable<bool> isEnabled = new NetworkVariable<bool>(false);

    public Animator animator;
    public AudioSource audioSource;
    public AudioClip clip;

    [Header("Interaction Events")]
    public UnityEvent onLeverEnabled;
    public UnityEvent onLeverDisabled;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animator.SetBool("enabled", isEnabled.Value);
    }

    private void OnEnable()
    {
        isEnabled.OnValueChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        isEnabled.OnValueChanged -= OnStateChanged;
    }

    public override void OnInteract()
    {
        ToggleServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void ToggleServerRpc()
    {
        isEnabled.Value = !isEnabled.Value;

        if (isEnabled.Value)
            onLeverEnabled.Invoke();
        else
            onLeverDisabled.Invoke();
    }

    //Local reaction to state change
    private void OnStateChanged(bool previous, bool current)
    {
        animator.SetBool("enabled", current);
        audioSource.PlayOneShot(clip);
        Debug.Log($"Lever is now {(current ? "enabled" : "disabled")}");
    }
}