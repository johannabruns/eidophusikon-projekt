using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class QuestOverlayManager : NetworkBehaviour
{
    [Header("Canvas")]
    public GameObject overlayRoot;
    public GameObject[] actPages =
        new GameObject[4];

    [Header("Button")]
    public Button closeButton;

    private readonly HashSet<ulong>
        readyClients = new();

    private int shownActIndex;
    private bool overlayIsActive;
    private bool buttonWasRegistered;

    public event Action<int>
        OnAllPlayersReady;

    public override void OnNetworkSpawn()
    {
        RegisterButton();
        HideOverlayLocally();
    }

    public override void OnNetworkDespawn()
    {
        UnregisterButton();
    }

    public void ShowActPage(
        int actIndex
    )
    {
        if (!IsServer)
            return;

        shownActIndex = actIndex;
        overlayIsActive = true;
        readyClients.Clear();

        ShowActPageRpc(actIndex);
    }

    [Rpc(SendTo.Everyone)]
    private void ShowActPageRpc(
        int actIndex
    )
    {
        RegisterButton();

        shownActIndex = actIndex;
        overlayIsActive = true;

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
        }

        for (int i = 0;
             i < actPages.Length;
             i++)
        {
            if (actPages[i] != null)
            {
                actPages[i].SetActive(
                    i == actIndex - 1
                );
            }
        }

        if (closeButton != null)
        {
            closeButton.interactable = true;
        }
    }

    private void RegisterButton()
    {
        if (buttonWasRegistered ||
            closeButton == null)
        {
            return;
        }

        closeButton.onClick.AddListener(
            OnCloseButtonPressed
        );

        buttonWasRegistered = true;
    }

    private void UnregisterButton()
    {
        if (!buttonWasRegistered ||
            closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveListener(
            OnCloseButtonPressed
        );

        buttonWasRegistered = false;
    }

    private void OnCloseButtonPressed()
    {
        if (!overlayIsActive)
            return;

        overlayIsActive = false;

        if (closeButton != null)
        {
            closeButton.interactable = false;
        }

        HideOverlayLocally();
        ConfirmPageClosedRpc();
    }

    private void HideOverlayLocally()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
            RpcInvokePermission.Everyone
    )]
    private void ConfirmPageClosedRpc(
        RpcParams rpcParams = default
    )
    {
        if (!overlayIsActive)
            return;

        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        readyClients.Add(
            senderClientId
        );

        if (!AreAllConnectedPlayersReady())
        {
            return;
        }

        overlayIsActive = false;

        OnAllPlayersReady?.Invoke(
            shownActIndex
        );
    }

    private bool AreAllConnectedPlayersReady()
    {
        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton
                .ConnectedClientsIds
                .Count == 0)
        {
            return false;
        }

        foreach (
            ulong clientId
            in NetworkManager.Singleton
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
}