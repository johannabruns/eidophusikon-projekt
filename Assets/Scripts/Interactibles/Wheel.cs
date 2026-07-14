using UnityEngine;

public class Wheel : AxisInteractible
{
    public Transform wheelTransform;

    protected override void VisualFeedback(float axisValue)
    {
        if (axisValue < 0)
        {
            if (target.transform.position != target.PointA.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }
        else if (axisValue > 0)
        {
            if (target.transform.position != target.PointB.position)
                wheelTransform.Rotate(0, 0, -axisValue * target.moveSpeed * Time.deltaTime * 30);
        }
    }
}
