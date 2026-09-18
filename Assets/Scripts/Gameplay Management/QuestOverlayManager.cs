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

    [Header("Audio")]
    public AudioSource pageAudioSource;
    public AudioClip pageTurnSound;

    [Range(0f, 1f)]
    public float pageTurnVolume = 1f;

    private readonly HashSet<ulong>
        readyClients = new();

    private int shownActIndex;

    private bool serverOverlayIsActive;
    private bool localOverlayIsActive;
    private bool buttonWasRegistered;

    public event Action<int>
        OnAllPlayersReady;

    public override void OnNetworkSpawn()
    {
        RegisterButton();
        PrepareAudioSource();
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
        serverOverlayIsActive = true;

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
        localOverlayIsActive = true;

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

        PlayPageTurnSound();
    }

    private void PrepareAudioSource()
    {
        if (pageAudioSource == null)
        {
            pageAudioSource = GetComponent<AudioSource>();
        }

        if (pageAudioSource == null)
        {
            pageAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        if (pageAudioSource != null)
        {
            pageAudioSource.ignoreListenerPause = true;
            pageAudioSource.playOnAwake = false;
        }
    }

    private void PlayPageTurnSound()
    {
        if (pageAudioSource == null ||
            pageTurnSound == null)
        {
            return;
        }

        pageAudioSource.PlayOneShot(
            pageTurnSound,
            pageTurnVolume
        );
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
        if (!localOverlayIsActive)
            return;

        localOverlayIsActive = false;

        if (closeButton != null)
        {
            closeButton.interactable = false;
        }

        PlayPageTurnSound();
        HideOverlayLocally();
        AudioListener.pause = false;
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
        if (!serverOverlayIsActive)
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

        serverOverlayIsActive = false;

        OnAllPlayersReady?.Invoke(
            shownActIndex
        );
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
}