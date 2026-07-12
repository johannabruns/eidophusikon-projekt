using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BoolStateObject : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inactiveSound;
    [SerializeField] private AudioClip activeSound;

    NetworkVariable<bool> isFlipped = new NetworkVariable<bool>(false);

    public UnityEvent onInactive;
    public UnityEvent onActive;

    private void OnEnable()
    {
        isFlipped.OnValueChanged += OnStateChange;
    }

    private void OnDisable()
    {
        isFlipped.OnValueChanged -= OnStateChange;
    }

    public override void OnNetworkSpawn()
    {
        SetSprite(isFlipped.Value);
    }


    public void ToggleState()
    {
        if (!IsOwner)
            return;
        ToggleStateRpc();
    }

    [Rpc(SendTo.Server)]
    private void ToggleStateRpc()
    {
        isFlipped.Value = !isFlipped.Value;
        
        if (isFlipped.Value)
            onActive.Invoke();
        else
            onInactive.Invoke();
    }

    public void SetActive()
    {
        if (!IsOwner)
            return;

        if (isFlipped.Value)
            return;

        SetActiveRpc();
    }

    [Rpc(SendTo.Server)]
    private void SetActiveRpc()
    {
        isFlipped.Value = true;
        onActive.Invoke();
    }

    public void SetInactive()
    {
        if (!IsOwner)
            return;

        if (!isFlipped.Value)
            return;
        SetInactiveRpc();
    }

    [Rpc(SendTo.Server)]
    private void SetInactiveRpc()
    {
        isFlipped.Value = false;
        onInactive.Invoke();
    }

    private void OnStateChange(bool previous, bool current)
    {
        SetSprite(current);

        if(audioSource == null || activeSound == null || inactiveSound == null)
            return;

        if (current)
            audioSource.PlayOneShot(activeSound);
        else
            audioSource.PlayOneShot(inactiveSound);
    }

    private void SetSprite(bool value)
    {
        if (value) 
            spriteRenderer.sprite = activeSprite;     
        else 
            spriteRenderer.sprite = inactiveSprite;
    }
}
