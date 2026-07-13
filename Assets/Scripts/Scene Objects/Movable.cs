using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class Movable : NetworkBehaviour
{
    public Transform PointA;
    public Transform PointB;

    public float moveSpeed = 2f;

    private Vector3 targetPosition;

    public bool moveOnAwake = false;

    public bool Automovement { get; private set; } = false;

    void Start()
    {
        targetPosition = PointA.position;

        if (moveOnAwake)
            StartMovement();
    }

    void FixedUpdate()
    {
        // Check if the object has reached the target position and switch to the other point
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            targetPosition = targetPosition == PointA.position ? PointB.position : PointA.position;

        if (Automovement) Move();
    }

    public void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void Move(float value)
    {
        Vector3 targetPosition = value < 0 ? PointA.position : PointB.position;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void StartMovement()
    {
        Automovement = true;
    }
    public void StopMovement()
    {
        Automovement = false;
    }

    private void OnDrawGizmos()
    {
        if (PointA == null || PointB == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(PointA.position, PointB.position);
        Gizmos.DrawWireSphere(PointA.position, 0.2f);
        Gizmos.DrawWireSphere(PointB.position, 0.2f);

    }
}
