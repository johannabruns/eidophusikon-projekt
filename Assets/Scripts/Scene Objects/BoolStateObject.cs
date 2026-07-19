using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class BoolStateObject : NetworkBehaviour
{
    [SerializeField] private string id;
    [Space]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inactiveSound;
    [SerializeField] private AudioClip activeSound;

    NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);

    public UnityEvent onInactive;
    public UnityEvent onActive;

    private void OnEnable()
    {
        isActive.OnValueChanged += OnStateChange;
    }

    private void OnDisable()
    {
        isActive.OnValueChanged -= OnStateChange;
    }

    public override void OnNetworkSpawn()
    {
        SetSprite(isActive.Value);
    }


    public void ToggleState()
    {
        ToggleStateRpc();
    }

    [Rpc(SendTo.Server)]
    private void ToggleStateRpc()
    {
        isActive.Value = !isActive.Value;
        
        if (isActive.Value)
            onActive.Invoke();
        else
            onInactive.Invoke();
    }

    public void SetActive(bool value)
    {
        SetActiveRpc(value);
    }

    [Rpc(SendTo.Server)]
    private void SetActiveRpc(bool value)
    {
        if (isActive.Value == value)
            return;
        isActive.Value = value;

        if (isActive.Value)
            onActive.Invoke();

        else if (!isActive.Value)
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
