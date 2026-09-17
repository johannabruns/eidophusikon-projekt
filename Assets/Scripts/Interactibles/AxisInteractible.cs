using Unity.Netcode;
using UnityEngine;

public class AxisInteractible : Interactible
{
    [Header("Movement")]
    public Movable target;
    public Movable secondaryTarget;
    public ItemSocket requiredSocket;
    public bool moveSequentially;

    [Header("Input")]
    public Collider2D rayTargetCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip soundEffect;

    protected NetworkVariable<float> axisInput =
        new(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    protected float previousAxisValue;

    private const float TargetTolerance = 0.02f;

    private void Start()
    {
        if (audioSource != null)
        {
            audioSource.clip = soundEffect;
        }
    }

    private void Update()
    {
        float effectiveAxisValue =
            GetEffectiveAxisValue();

        if (IsServer &&
            effectiveAxisValue != 0f)
        {
            MoveTargets(effectiveAxisValue);
        }

        AudioFeedback(effectiveAxisValue);
        VisualFeedback(effectiveAxisValue);
    }

    public void Turn(float axisValue)
    {
        SetAxisServerRpc(axisValue);
    }

    public void Stop()
    {
        SetAxisServerRpc(0f);
    }

    private float GetEffectiveAxisValue()
    {
        if (!CanOperate())
            return 0f;

        if (!CanMoveInDirection(
                axisInput.Value
            ))
        {
            return 0f;
        }

        return axisInput.Value;
    }

    private bool CanOperate()
    {
        return requiredSocket == null ||
               !requiredSocket.IstLeer;
    }

    protected bool CanMoveInDirection(
        float axisValue
    )
    {
        if (target == null ||
            axisValue == 0f)
        {
            return false;
        }

        if (secondaryTarget == null)
        {
            return axisValue < 0f
                ? !IsAtPoint(
                    target,
                    target.PointA
                )
                : !IsAtPoint(
                    target,
                    target.PointB
                );
        }

        if (!moveSequentially)
        {
            if (axisValue < 0f)
            {
                return
                    !IsAtPoint(
                        target,
                        target.PointA
                    ) ||
                    !IsAtPoint(
                        secondaryTarget,
                        secondaryTarget.PointA
                    );
            }

            return
                !IsAtPoint(
                    target,
                    target.PointB
                ) ||
                !IsAtPoint(
                    secondaryTarget,
                    secondaryTarget.PointB
                );
        }

        if (axisValue > 0f)
        {
            return
                !IsAtPoint(
                    target,
                    target.PointB
                ) ||
                !IsAtPoint(
                    secondaryTarget,
                    secondaryTarget.PointB
                );
        }

        return
            !IsAtPoint(
                secondaryTarget,
                secondaryTarget.PointA
            ) ||
            !IsAtPoint(
                target,
                target.PointA
            );
    }

    private void MoveTargets(
        float axisValue
    )
    {
        if (target == null)
            return;

        if (secondaryTarget == null)
        {
            target.Move(axisValue);
            return;
        }

        if (!moveSequentially)
        {
            target.Move(axisValue);
            secondaryTarget.Move(axisValue);
            return;
        }

        if (axisValue > 0f)
        {
            if (!IsAtPoint(
                    target,
                    target.PointB
                ))
            {
                target.Move(axisValue);
            }
            else if (!IsAtPoint(
                         secondaryTarget,
                         secondaryTarget.PointB
                     ))
            {
                secondaryTarget.Move(
                    axisValue
                );
            }

            return;
        }

        if (!IsAtPoint(
                secondaryTarget,
                secondaryTarget.PointA
            ))
        {
            secondaryTarget.Move(
                axisValue
            );
        }
        else if (!IsAtPoint(
                     target,
                     target.PointA
                 ))
        {
            target.Move(axisValue);
        }
    }

    private bool IsAtPoint(
        Movable movable,
        Transform point
    )
    {
        if (movable == null ||
            point == null)
        {
            return true;
        }

        return Vector3.Distance(
            movable.transform.position,
            point.position
        ) <= TargetTolerance;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission = RpcInvokePermission.Everyone
    )]
    private void SetAxisServerRpc(
        float axisValue
    )
    {
        if (requiredSocket != null &&
            requiredSocket.IstLeer)
        {
            axisInput.Value = 0f;
            return;
        }

        axisInput.Value =
            Mathf.Clamp(axisValue, -1f, 1f);
    }

    private void AudioFeedback(
        float axisValue
    )
    {
        if (audioSource == null)
            return;

        if (axisValue != 0f)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        previousAxisValue = axisValue;
    }

    protected virtual void VisualFeedback(
        float axisValue
    )
    {
    }
}