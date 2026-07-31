using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Interactible for moving GameObjects. target Objects needs a Movable component.
/// </summary>
public class AxisInteractible : Interactible
{
    public Movable target;
    public Collider2D rayTargetCollider;
    public AudioSource audioSource;
    public AudioClip soundEffect;

    // Synced automatically to every client; server is the only writer.
    protected NetworkVariable<float> axisInput = new NetworkVariable<float>(0f);

    protected float previousAxisValue = 0f;

    private void Start()
    {
        audioSource.clip = soundEffect;
    }

    private void Update()
    {
        if (IsServer && target != null && axisInput.Value != 0f)
            target.Move(axisInput.Value);

        // Every peer (host + clients) drives its own local visuals/audio
        // off the synced value, at its own framerate.
        AudioFeedback(axisInput.Value);
        VisualFeedback(axisInput.Value);
    }

    public void Turn(float axisValue) => SetAxisServerRpc(axisValue);
    public void Stop() => SetAxisServerRpc(0f);

    [Rpc(SendTo.Server)]
    private void SetAxisServerRpc(float axisValue)
    {
        axisInput.Value = axisValue;
    }

    private void AudioFeedback(float axisValue)
    {
        if (axisValue < 0)
        {
            if (previousAxisValue > axisValue || !audioSource.isPlaying)
                audioSource.Play();
        }
        else if (axisValue > 0)
        {
            if (previousAxisValue < axisValue || !audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        previousAxisValue = axisValue;
    }

    /// <summary>
    /// Child classes can override this method to provide visual feedback based on the axis value. This method is called every frame in Update().
    /// </summary>
    protected virtual void VisualFeedback(float axisValue)
    {
    }
}