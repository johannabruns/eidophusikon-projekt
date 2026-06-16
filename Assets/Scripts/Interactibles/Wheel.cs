using UnityEngine;
using UnityEngine.InputSystem;

public class Wheel : Interactible
{
    public Movable target;
    public Transform wheelTransform;
    public Collider2D wheelCollider;

    public AudioSource audioSource;
    public AudioClip soundEffect;

    private float previousAxisValue = 0f;

    private void Start()
    {
        audioSource.clip = soundEffect;
    }
    public void Turn(float axisValue)
    {
        if (axisValue < 0)
        {
            if (previousAxisValue > axisValue || !audioSource.isPlaying)
                audioSource.Play();

            previousAxisValue = axisValue;

            //TODO: decouple movable logic from wheel class into movable so it may be used for other cases aswell.
            target.transform.position = Vector2.MoveTowards(target.transform.position, target.PointA.position, target.moveSpeed * Time.deltaTime);

            if (target.transform.position != target.PointA.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }

        else if (axisValue > 0)
        {
            if (previousAxisValue < axisValue || !audioSource.isPlaying)
                audioSource.Play();

            previousAxisValue = axisValue;

            //TODO: decouple movable logic from wheel class into movable so it may be used for other cases aswell.
            target.transform.position = Vector2.MoveTowards(target.transform.position, target.PointB.position, target.moveSpeed * Time.deltaTime);

            if (target.transform.position != target.PointB.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
