using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class StopMotionRope : Interactible
{
    [Header("Raycast")]
    public Collider2D rayTargetCollider;

    [Header("Stop Motion Frames")]
    public GameObject[] ropeFrames;

    [Min(0)]
    public int startingFrame;

    public bool invertScrollDirection;

    [Min(0f)]
    public float scrollCooldown = 0.08f;

    [Header("Curtain")]
    public TheaterManager theaterManager;

    public bool longestStateOpensCurtain = true;

    public NetworkVariable<int> currentFrame =
        new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private float lastScrollTime;

    public bool IsShortest =>
        currentFrame.Value == 0;

    public bool IsLongest =>
        ropeFrames.Length > 0 &&
        currentFrame.Value ==
        ropeFrames.Length - 1;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentFrame.OnValueChanged +=
            OnFrameChanged;

        if (IsServer &&
            ropeFrames.Length > 0)
        {
            currentFrame.Value =
                Mathf.Clamp(
                    startingFrame,
                    0,
                    ropeFrames.Length - 1
                );
        }

        ApplyFrame(currentFrame.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentFrame.OnValueChanged -=
            OnFrameChanged;

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsSpawned ||
            player == null ||
            Mouse.current == null ||
            Camera.main == null ||
            rayTargetCollider == null)
        {
            return;
        }

        PlayerInteraction interaction =
            player.GetComponent<PlayerInteraction>();

        if (interaction == null ||
            !interaction.IsOwner)
        {
            return;
        }

        float scrollValue =
            Mouse.current.scroll
                .ReadValue().y;

        if (Mathf.Abs(scrollValue) <=
            0.01f)
        {
            return;
        }

        if (Time.time - lastScrollTime <
            scrollCooldown)
        {
            return;
        }

        if (!IsMouseOverTarget())
            return;

        int direction =
            scrollValue > 0f
                ? 1
                : -1;

        if (invertScrollDirection)
        {
            direction *= -1;
        }

        lastScrollTime = Time.time;

        ChangeFrameServerRpc(direction);
    }

    private bool IsMouseOverTarget()
    {
        Ray mouseRay =
            Camera.main.ScreenPointToRay(
                Mouse.current.position
                    .ReadValue()
            );

        RaycastHit2D[] hits =
            Physics2D.GetRayIntersectionAll(
                mouseRay,
                100f
            );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider ==
                rayTargetCollider)
            {
                return true;
            }
        }

        return false;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    private void ChangeFrameServerRpc(
        int direction
    )
    {
        if (ropeFrames.Length == 0)
            return;

        int nextFrame =
            Mathf.Clamp(
                currentFrame.Value +
                direction,
                0,
                ropeFrames.Length - 1
            );

        currentFrame.Value =
            nextFrame;
    }

    private void OnFrameChanged(
        int previous,
        int current
    )
    {
        ApplyFrame(current);

        if (IsServer)
        {
            UpdateCurtain(current);
        }
    }

    private void UpdateCurtain(int frame)
    {
        if (theaterManager == null ||
            ropeFrames.Length == 0)
        {
            return;
        }

        bool reachedShortest =
            frame == 0;

        bool reachedLongest =
            frame ==
            ropeFrames.Length - 1;

        if (longestStateOpensCurtain)
        {
            if (reachedLongest)
            {
                theaterManager.OpenCurtains();
            }
            else if (reachedShortest)
            {
                theaterManager.CloseCurtains();
            }
        }
        else
        {
            if (reachedLongest)
            {
                theaterManager.CloseCurtains();
            }
            else if (reachedShortest)
            {
                theaterManager.OpenCurtains();
            }
        }
    }

    private void ApplyFrame(int frame)
    {
        if (ropeFrames.Length == 0)
            return;

        int safeFrame =
            Mathf.Clamp(
                frame,
                0,
                ropeFrames.Length - 1
            );

        for (
            int i = 0;
            i < ropeFrames.Length;
            i++
        )
        {
            if (ropeFrames[i] != null)
            {
                ropeFrames[i].SetActive(
                    i <= safeFrame
                );
            }
        }
    }
}