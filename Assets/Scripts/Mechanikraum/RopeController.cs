using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class SeilSystem
{
    [Header("Mechanikraum - Vertikaler Zug")]
    public SpriteRenderer vertikalesSeil;
    public Transform seilEnde;

    [Header("Mechanikraum - Horizontales Fließband")]
    public SpriteRenderer horizontalesSeil;

    [Header("Bühne - Vertikaler Zug")]
    public SpriteRenderer buehnenSeil;
    public Transform buehnenSeilEnde;

    [Header("Bühne - Ziel")]
    public Transform buehnenZielpunkt;

    [Min(0.01f)]
    public float zielToleranz = 0.25f;
}

public class RopeController : NetworkBehaviour
{
    [Header("Die 3 Seilzüge")]
    public SeilSystem[] seilSysteme =
        new SeilSystem[3];

    [Header("Einstellungen Vertikal")]
    public float scrollGeschwindigkeit = 1f;
    public float minLaenge = 1f;
    public float maxLaenge = 10f;

    [Header("Einstellungen Horizontal")]
    public float horizontalesScrollTempo = 0.5f;

    [Header("Zielstatus")]
    [SerializeField]
    private bool alleBuehnenSeileImZiel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ropeMoveSound;

    [Min(0f)]
    public float soundCooldown = 0.08f;

    private int aktivesSeilIndex = -1;

    private float lastSoundTime =
        float.NegativeInfinity;

    private bool audioReady;

    public NetworkVariable<float> seil1Laenge =
        new NetworkVariable<float>(1f);

    public NetworkVariable<float> seil2Laenge =
        new NetworkVariable<float>(1f);

    public NetworkVariable<float> seil3Laenge =
        new NetworkVariable<float>(1f);

    public NetworkVariable<float> seil1Offset =
        new NetworkVariable<float>(0f);

    public NetworkVariable<float> seil2Offset =
        new NetworkVariable<float>(0f);

    public NetworkVariable<float> seil3Offset =
        new NetworkVariable<float>(0f);

    public NetworkVariable<bool> seil1Eingerastet =
        new NetworkVariable<bool>(false);

    public NetworkVariable<bool> seil2Eingerastet =
        new NetworkVariable<bool>(false);

    public NetworkVariable<bool> seil3Eingerastet =
        new NetworkVariable<bool>(false);

    public bool AlleBuehnenSeileImZiel =>
        alleBuehnenSeileImZiel;

    public override void OnNetworkSpawn()
    {
        seil1Laenge.OnValueChanged +=
            HandleRopeMovement;

        seil2Laenge.OnValueChanged +=
            HandleRopeMovement;

        seil3Laenge.OnValueChanged +=
            HandleRopeMovement;

        seil1Offset.OnValueChanged +=
            HandleRopeMovement;

        seil2Offset.OnValueChanged +=
            HandleRopeMovement;

        seil3Offset.OnValueChanged +=
            HandleRopeMovement;

        audioReady = true;
    }

    public override void OnNetworkDespawn()
    {
        audioReady = false;

        seil1Laenge.OnValueChanged -=
            HandleRopeMovement;

        seil2Laenge.OnValueChanged -=
            HandleRopeMovement;

        seil3Laenge.OnValueChanged -=
            HandleRopeMovement;

        seil1Offset.OnValueChanged -=
            HandleRopeMovement;

        seil2Offset.OnValueChanged -=
            HandleRopeMovement;

        seil3Offset.OnValueChanged -=
            HandleRopeMovement;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            aktivesSeilIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            aktivesSeilIndex = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            aktivesSeilIndex = 2;
        }

        if (aktivesSeilIndex != -1)
        {
            float scrollInput =
                Input.mouseScrollDelta.y;

            if (scrollInput != 0f &&
                !IsRopeLocked(
                    aktivesSeilIndex
                ))
            {
                UpdateRopeServerRpc(
                    aktivesSeilIndex,
                    scrollInput
                );
            }
        }

        ApplyVisuals(
            0,
            seil1Laenge.Value,
            seil1Offset.Value
        );

        ApplyVisuals(
            1,
            seil2Laenge.Value,
            seil2Offset.Value
        );

        ApplyVisuals(
            2,
            seil3Laenge.Value,
            seil3Offset.Value
        );

        if (IsServer)
        {
            CheckRopesForSnapping();
        }

        alleBuehnenSeileImZiel =
            seil1Eingerastet.Value &&
            seil2Eingerastet.Value &&
            seil3Eingerastet.Value;
    }

    private void HandleRopeMovement(
        float previous,
        float current
    )
    {
        if (!audioReady ||
            Mathf.Approximately(
                previous,
                current
            ))
        {
            return;
        }

        if (audioSource == null ||
            ropeMoveSound == null ||
            Time.unscaledTime -
            lastSoundTime <
            soundCooldown)
        {
            return;
        }

        lastSoundTime =
            Time.unscaledTime;

        audioSource.PlayOneShot(
            ropeMoveSound
        );
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    public void UpdateRopeServerRpc(
        int index,
        float scrollInput
    )
    {
        if (seilSysteme == null ||
            index < 0 ||
            index >= seilSysteme.Length ||
            IsRopeLocked(index))
        {
            return;
        }

        float currentLength =
            GetRopeLength(index);

        float nextLength =
            Mathf.Clamp(
                currentLength +
                scrollInput *
                scrollGeschwindigkeit,
                minLaenge,
                maxLaenge
            );

        float nextOffset =
            GetRopeOffset(index) +
            scrollInput *
            horizontalesScrollTempo;

        if (TryGetTargetLength(
                index,
                out float targetLength
            ) &&
            ReachesTarget(
                index,
                currentLength,
                nextLength,
                targetLength
            ))
        {
            SetRopeLength(
                index,
                targetLength
            );

            SetRopeOffset(
                index,
                nextOffset
            );

            SetRopeLocked(
                index,
                true
            );

            return;
        }

        SetRopeLength(
            index,
            nextLength
        );

        SetRopeOffset(
            index,
            nextOffset
        );
    }

    public bool IsStageRopeAtTarget(
        int index
    )
    {
        return IsRopeLocked(index);
    }

    public bool AreAllStageRopesAtTargets()
    {
        return
            seil1Eingerastet.Value &&
            seil2Eingerastet.Value &&
            seil3Eingerastet.Value;
    }

    private void CheckRopesForSnapping()
    {
        for (int index = 0;
             index < seilSysteme.Length;
             index++)
        {
            if (IsRopeLocked(index))
            {
                continue;
            }

            TrySnapRopeByWorldPosition(
                index
            );
        }
    }

    private void TrySnapRopeByWorldPosition(
        int index
    )
    {
        SeilSystem system =
            seilSysteme[index];

        if (system == null ||
            system.buehnenSeilEnde == null ||
            system.buehnenZielpunkt == null)
        {
            return;
        }

        float tolerance =
            Mathf.Max(
                0.01f,
                system.zielToleranz
            );

        float distance =
            Vector3.Distance(
                system.buehnenSeilEnde.position,
                system.buehnenZielpunkt.position
            );

        if (distance > tolerance)
        {
            return;
        }

        if (TryGetTargetLength(
            index,
            out float targetLength
        ))
        {
            SetRopeLength(
                index,
                targetLength
            );
        }

        SetRopeLocked(
            index,
            true
        );
    }

    private bool ReachesTarget(
        int index,
        float currentLength,
        float nextLength,
        float targetLength
    )
    {
        float tolerance =
            Mathf.Max(
                0.01f,
                seilSysteme[index]
                    .zielToleranz
            );

        float currentDistance =
            Mathf.Abs(
                currentLength -
                targetLength
            );

        float nextDistance =
            Mathf.Abs(
                nextLength -
                targetLength
            );

        bool alreadyInsideRange =
            currentDistance <=
            tolerance;

        bool movingTowardTarget =
            nextDistance <
            currentDistance;

        bool nextInsideRange =
            nextDistance <=
            tolerance;

        bool crossedTarget =
            currentLength <
                targetLength &&
            nextLength >=
                targetLength ||
            currentLength >
                targetLength &&
            nextLength <=
                targetLength;

        return
            alreadyInsideRange ||
            crossedTarget ||
            movingTowardTarget &&
            nextInsideRange;
    }

    private bool TryGetTargetLength(
        int index,
        out float targetLength
    )
    {
        targetLength = 0f;

        if (seilSysteme == null ||
            index < 0 ||
            index >= seilSysteme.Length)
        {
            return false;
        }

        SeilSystem system =
            seilSysteme[index];

        if (system == null ||
            system.buehnenSeilEnde == null ||
            system.buehnenZielpunkt == null)
        {
            return false;
        }

        Transform endpointParent =
            system.buehnenSeilEnde.parent;

        if (endpointParent == null)
        {
            return false;
        }

        Vector3 targetLocalPosition =
            endpointParent.InverseTransformPoint(
                system.buehnenZielpunkt
                    .position
            );

        targetLength =
            Mathf.Clamp(
                -targetLocalPosition.y,
                minLaenge,
                maxLaenge
            );

        return true;
    }

    private bool IsRopeLocked(
        int index
    )
    {
        if (index == 0)
        {
            return
                seil1Eingerastet.Value;
        }

        if (index == 1)
        {
            return
                seil2Eingerastet.Value;
        }

        if (index == 2)
        {
            return
                seil3Eingerastet.Value;
        }

        return true;
    }

    private void SetRopeLocked(
        int index,
        bool locked
    )
    {
        if (index == 0)
        {
            seil1Eingerastet.Value =
                locked;
        }
        else if (index == 1)
        {
            seil2Eingerastet.Value =
                locked;
        }
        else if (index == 2)
        {
            seil3Eingerastet.Value =
                locked;
        }
    }

    private float GetRopeLength(
        int index
    )
    {
        if (index == 0)
        {
            return seil1Laenge.Value;
        }

        if (index == 1)
        {
            return seil2Laenge.Value;
        }

        return seil3Laenge.Value;
    }

    private void SetRopeLength(
        int index,
        float value
    )
    {
        if (index == 0)
        {
            seil1Laenge.Value =
                value;
        }
        else if (index == 1)
        {
            seil2Laenge.Value =
                value;
        }
        else if (index == 2)
        {
            seil3Laenge.Value =
                value;
        }
    }

    private float GetRopeOffset(
        int index
    )
    {
        if (index == 0)
        {
            return seil1Offset.Value;
        }

        if (index == 1)
        {
            return seil2Offset.Value;
        }

        return seil3Offset.Value;
    }

    private void SetRopeOffset(
        int index,
        float value
    )
    {
        if (index == 0)
        {
            seil1Offset.Value =
                value;
        }
        else if (index == 1)
        {
            seil2Offset.Value =
                value;
        }
        else if (index == 2)
        {
            seil3Offset.Value =
                value;
        }
    }

    private void ApplyVisuals(
        int index,
        float laenge,
        float offset
    )
    {
        if (seilSysteme == null ||
            index < 0 ||
            index >= seilSysteme.Length)
        {
            return;
        }

        SeilSystem aktiv =
            seilSysteme[index];

        if (aktiv == null)
        {
            return;
        }

        if (aktiv.vertikalesSeil != null)
        {
            Vector2 groesse =
                aktiv.vertikalesSeil.size;

            groesse.y = laenge;

            aktiv.vertikalesSeil.size =
                groesse;

            if (aktiv.seilEnde != null)
            {
                aktiv.seilEnde.localPosition =
                    new Vector3(
                        0f,
                        -laenge,
                        0f
                    );
            }
        }

        if (aktiv.horizontalesSeil != null)
        {
            Vector2 currentOffset =
                aktiv.horizontalesSeil
                    .material
                    .mainTextureOffset;

            currentOffset.y = offset;

            aktiv.horizontalesSeil
                .material
                .mainTextureOffset =
                    currentOffset;
        }

        if (aktiv.buehnenSeil != null)
        {
            Vector2 groesse =
                aktiv.buehnenSeil.size;

            groesse.y = laenge;

            aktiv.buehnenSeil.size =
                groesse;

            if (aktiv.buehnenSeilEnde != null)
            {
                aktiv.buehnenSeilEnde
                    .localPosition =
                        new Vector3(
                            0f,
                            -laenge,
                            0f
                        );
            }
        }
    }
}