using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    [Header("UI Basis")]
    public GameObject startMenuPanel;
    public GameObject buttonContainer;

    [Header("Eingabe & Anzeige")]
    public TMP_InputField codeEingabeFeld;
    public TextMeshProUGUI codeAnzeigeText;

    [Header("Lade-Sequenz")]
    public LoadingScreen loadingScreen;

    [Min(0f)]
    public float loadingDuration = 3f;

    [Min(1f)]
    public float connectionTimeout = 20f;

    [Header("Regiebuch")]
    public GameObject zweiterCanvas;

    private bool isStarting;

    private async void Start()
    {
        AudioListener.pause = false;

        if (loadingScreen != null)
        {
            loadingScreen.HideLoadingScreen();
        }

        if (zweiterCanvas != null)
        {
            zweiterCanvas.SetActive(false);
        }

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton
                .OnClientConnectedCallback +=
                OnPlayerConnected;
        }
    }

    private void OnDestroy()
    {
        AudioListener.pause = false;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton
                .OnClientConnectedCallback -=
                OnPlayerConnected;
        }
    }

    public async void CreateRelayAndStartHost()
    {
        if (isStarting)
        {
            return;
        }

        if (codeAnzeigeText != null)
        {
            codeAnzeigeText.text =
                "Erstelle Server...";
        }

        try
        {
            Allocation allocation =
                await RelayService.Instance
                    .CreateAllocationAsync(3);

            string joinCode =
                await RelayService.Instance
                    .GetJoinCodeAsync(
                        allocation.AllocationId
                    );

            if (codeAnzeigeText != null)
            {
                codeAnzeigeText.text =
                    "Code: " + joinCode;
            }

            NetworkManager networkManager =
                NetworkManager.Singleton;

            if (networkManager == null)
            {
                Debug.LogError(
                    "Kein NetworkManager gefunden."
                );

                return;
            }

            networkManager
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    allocation.ToRelayServerData(
                        "dtls"
                    )
                );

            bool hostStarted =
                networkManager.StartHost();

            if (!hostStarted)
            {
                Debug.LogError(
                    "Der Host konnte nicht gestartet werden."
                );
            }
        }
        catch (RelayServiceException exception)
        {
            if (codeAnzeigeText != null)
            {
                codeAnzeigeText.text =
                    "Fehler bei der Verbindung!";
            }

            Debug.LogError(exception);
        }
    }

    public async void JoinRelayAndStartClient()
    {
        if (isStarting)
        {
            return;
        }

        if (codeEingabeFeld != null)
        {
            codeEingabeFeld.interactable =
                false;
        }

        try
        {
            JoinAllocation joinAllocation =
                await RelayService.Instance
                    .JoinAllocationAsync(
                        codeEingabeFeld.text
                    );

            NetworkManager networkManager =
                NetworkManager.Singleton;

            if (networkManager == null)
            {
                Debug.LogError(
                    "Kein NetworkManager gefunden."
                );

                RestoreClientMenu();
                return;
            }

            networkManager
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    joinAllocation
                        .ToRelayServerData(
                            "dtls"
                        )
                );

            bool clientStarted =
                networkManager.StartClient();

            if (!clientStarted)
            {
                Debug.LogError(
                    "Der Client konnte nicht gestartet werden."
                );

                RestoreClientMenu();
                return;
            }

            // Der Client zeigt den Ladescreen sofort.
            // Er ist dadurch nicht mehr von einem
            // möglicherweise verpassten Callback abhängig.
            TryStartGameSequence(
                waitForLocalConnection: true
            );
        }
        catch (RelayServiceException exception)
        {
            Debug.LogError(
                "Falscher Code oder Verbindung " +
                "fehlgeschlagen: " +
                exception
            );

            RestoreClientMenu();
        }
    }

    private void OnPlayerConnected(
        ulong clientId
    )
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            !networkManager.IsHost)
        {
            return;
        }

        // Der Host soll erst starten, wenn ein
        // anderer Spieler beigetreten ist.
        if (clientId ==
            networkManager.LocalClientId)
        {
            return;
        }

        TryStartGameSequence(
            waitForLocalConnection: false
        );
    }

    private void TryStartGameSequence(
        bool waitForLocalConnection
    )
    {
        if (isStarting)
        {
            return;
        }

        isStarting = true;

        StartCoroutine(
            StartGameSequence(
                waitForLocalConnection
            )
        );
    }

    private IEnumerator StartGameSequence(
        bool waitForLocalConnection
    )
    {
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(false);
        }

        if (zweiterCanvas != null)
        {
            zweiterCanvas.SetActive(false);
        }

        AudioListener.pause = true;

        if (loadingScreen != null)
        {
            loadingScreen.ShowLoadingScreen();
        }

        float safeLoadingDuration =
            Mathf.Max(
                0.01f,
                loadingDuration
            );

        float elapsedTime = 0f;
        float totalWaitTime = 0f;

        while (true)
        {
            float deltaTime =
                Time.unscaledDeltaTime;

            elapsedTime += deltaTime;
            totalWaitTime += deltaTime;

            if (loadingScreen != null)
            {
                loadingScreen.UpdateProgress(
                    Mathf.Clamp01(
                        elapsedTime /
                        safeLoadingDuration
                    )
                );
            }

            bool minimumDurationFinished =
                elapsedTime >=
                safeLoadingDuration;

            bool connectionReady =
                !waitForLocalConnection ||
                (
                    NetworkManager.Singleton !=
                    null &&
                    NetworkManager.Singleton
                        .IsConnectedClient
                );

            if (minimumDurationFinished &&
                connectionReady)
            {
                break;
            }

            if (waitForLocalConnection &&
                !connectionReady &&
                totalWaitTime >=
                connectionTimeout)
            {
                Debug.LogError(
                    "Zeitüberschreitung beim " +
                    "Verbinden mit dem Host."
                );

                RestoreClientMenu();
                yield break;
            }

            yield return null;
        }

        if (loadingScreen != null)
        {
            loadingScreen.UpdateProgress(1f);
        }

        yield return
            new WaitForSecondsRealtime(
                0.25f
            );

        if (startMenuPanel != null)
        {
            startMenuPanel.SetActive(false);
        }

        if (loadingScreen != null)
        {
            loadingScreen.HideLoadingScreen();
        }

        if (zweiterCanvas != null)
        {
            zweiterCanvas.SetActive(true);
        }
    }

    private void RestoreClientMenu()
    {
        AudioListener.pause = false;
        isStarting = false;

        if (loadingScreen != null)
        {
            loadingScreen.HideLoadingScreen();
        }

        if (buttonContainer != null)
        {
            buttonContainer.SetActive(true);
        }

        if (codeEingabeFeld != null)
        {
            codeEingabeFeld.interactable =
                true;
        }
    }
}