using UnityEngine;

public class Movable : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;

    public float moveSpeed = 2f;

    private Vector3 targetPosition;

    public bool moveOnAwake = false;

    public bool IsMoving { get; private set; } = false;

    void Start()
    {
        targetPosition = PointA.position;

        if (moveOnAwake)
            StartMovement();
    }

    void Update()
    {
        if (IsMoving)
        {

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                targetPosition = targetPosition == PointA.position ? PointB.position : PointA.position;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        }
    }

    public void StartMovement()
    {
        IsMoving = true;
    }
    public void StopMovement()
    {
        IsMoving = false;
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
