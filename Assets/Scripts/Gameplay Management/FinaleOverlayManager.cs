using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinaleOverlayManager : NetworkBehaviour
{
    [Header("Finale Canvas")]
    public GameObject finaleOverlayRoot;
    public Button curtainCloseButton;

    [Header("Scene")]
    public string startSceneName = "Start";

    [Min(0f)]
    public float returnDelay = 0.75f;

    private readonly HashSet<ulong>
        readyClients = new();

    private bool serverFinaleIsActive;
    private bool localFinaleIsActive;
    private bool localPlayerConfirmed;
    private bool buttonWasRegistered;
    private bool returnStarted;

    public override void OnNetworkSpawn()
    {
        RegisterButton();
        HideFinaleLocally();

        if (IsServer)
        {
            QuestManager.OnFinaleComplete +=
                ShowFinale;
        }
    }

    public override void OnNetworkDespawn()
    {
        UnregisterButton();

        if (IsServer)
        {
            QuestManager.OnFinaleComplete -=
                ShowFinale;
        }
    }

    private void ShowFinale()
    {
        if (!IsServer ||
            serverFinaleIsActive ||
            returnStarted)
        {
            return;
        }

        serverFinaleIsActive = true;
        readyClients.Clear();

        ShowFinaleRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ShowFinaleRpc()
    {
        RegisterButton();

        localFinaleIsActive = true;
        localPlayerConfirmed = false;

        if (finaleOverlayRoot != null)
        {
            finaleOverlayRoot.SetActive(
                true
            );
        }

        if (curtainCloseButton != null)
        {
            curtainCloseButton.interactable =
                true;
        }
    }

    private void RegisterButton()
    {
        if (buttonWasRegistered ||
            curtainCloseButton == null)
        {
            return;
        }

        curtainCloseButton
            .onClick
            .AddListener(
                OnCurtainClosePressed
            );

        buttonWasRegistered = true;
    }

    private void UnregisterButton()
    {
        if (!buttonWasRegistered ||
            curtainCloseButton == null)
        {
            return;
        }

        curtainCloseButton
            .onClick
            .RemoveListener(
                OnCurtainClosePressed
            );

        buttonWasRegistered = false;
    }

    private void OnCurtainClosePressed()
    {
        if (!localFinaleIsActive ||
            localPlayerConfirmed ||
            returnStarted)
        {
            return;
        }

        localPlayerConfirmed = true;

        if (curtainCloseButton != null)
        {
            curtainCloseButton.interactable =
                false;
        }

        ConfirmFinaleRpc();
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    private void ConfirmFinaleRpc(
        RpcParams rpcParams = default
    )
    {
        if (!serverFinaleIsActive ||
            returnStarted)
        {
            return;
        }

        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        readyClients.Add(
            senderClientId
        );

        if (!AreAllConnectedPlayersReady())
        {
            return;
        }

        serverFinaleIsActive = false;
        returnStarted = true;

        ReturnToStartRpc();
    }

    private bool AreAllConnectedPlayersReady()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            networkManager
                .ConnectedClientsIds
                .Count == 0)
        {
            return false;
        }

        foreach (
            ulong clientId
            in networkManager
                .ConnectedClientsIds
        )
        {
            if (!readyClients.Contains(
                    clientId
                ))
            {
                return false;
            }
        }

        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void ReturnToStartRpc()
    {
        localFinaleIsActive = false;
        returnStarted = true;

        StartCoroutine(
            ReturnToStartRoutine()
        );
    }

    private IEnumerator ReturnToStartRoutine()
    {
        yield return new WaitForSecondsRealtime(
            returnDelay
        );

        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager != null)
        {
            GameObject networkManagerObject =
                networkManager.gameObject;

            networkManager.Shutdown();

            if (networkManagerObject != null)
            {
                Destroy(
                    networkManagerObject
                );
            }
        }

        SceneManager.LoadScene(
            startSceneName
        );
    }

    private void HideFinaleLocally()
    {
        localFinaleIsActive = false;

        if (finaleOverlayRoot != null)
        {
            finaleOverlayRoot.SetActive(
                false
            );
        }
    }
}