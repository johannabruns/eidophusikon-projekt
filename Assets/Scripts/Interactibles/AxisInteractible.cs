using Unity.Netcode;
using UnityEngine;

public class AxisInteractible : Interactible
{
    public Movable target;
    public Transform wheelTransform;
    public Collider2D wheelCollider;
    public AudioSource audioSource;
    public AudioClip soundEffect;

    // Synced automatically to every client; server is the only writer.
    private NetworkVariable<float> axisInput = new NetworkVariable<float>(0f);

    private float previousAxisValue = 0f;

    private void Start()
    {
        audioSource.clip = soundEffect;
    }

    private void Update()
    {
        if (IsServer && axisInput.Value != 0f)
            target.Move(axisInput.Value);

        // Every peer (host + clients) drives its own local visuals/audio
        // off the synced value, at its own framerate.
        HandleFeedback(axisInput.Value);
    }

    public void Turn(float axisValue) => SetAxisServerRpc(axisValue);
    public void Stop() => SetAxisServerRpc(0f);

    [Rpc(SendTo.Server)]
    private void SetAxisServerRpc(float axisValue)
    {
        axisInput.Value = axisValue;
    }

    private void HandleFeedback(float axisValue)
    {
        if (axisValue < 0)
        {
            if (previousAxisValue > axisValue || !audioSource.isPlaying)
                audioSource.Play();
            if (target.transform.position != target.PointA.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }
        else if (axisValue > 0)
        {
            if (previousAxisValue < axisValue || !audioSource.isPlaying)
                audioSource.Play();
            if (target.transform.position != target.PointB.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        previousAxisValue = axisValue;
    }
}