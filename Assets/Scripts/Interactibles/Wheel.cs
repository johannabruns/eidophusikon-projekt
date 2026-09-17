using UnityEngine;

public class Wheel : AxisInteractible
{
    public Transform wheelTransform;

    protected override void VisualFeedback(
        float axisValue
    )
    {
        if (target == null ||
            wheelTransform == null ||
            axisValue == 0f)
        {
            return;
        }

        wheelTransform.Rotate(
            0f,
            0f,
            -axisValue *
            target.moveSpeed *
            Time.deltaTime *
            30f
        );
    }
}