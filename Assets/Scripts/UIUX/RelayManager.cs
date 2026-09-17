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
    [Min(0f)] public float loadingDuration = 3f;

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
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                OnPlayerConnected;
        }
    }

    private void OnDestroy()
    {
        AudioListener.pause = false;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -=
                OnPlayerConnected;
        }
    }

    public async void CreateRelayAndStartHost()
    {
        codeAnzeigeText.text = "Erstelle Server...";

        try
        {
            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(
                    allocation.AllocationId
                );

            codeAnzeigeText.text = "Code: " + joinCode;

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    allocation.ToRelayServerData("dtls")
                );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException exception)
        {
            codeAnzeigeText.text =
                "Fehler bei der Verbindung!";

            Debug.LogError(exception);
        }
    }

    public async void JoinRelayAndStartClient()
    {
        codeEingabeFeld.interactable = false;

        try
        {
            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(
                    codeEingabeFeld.text
                );

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    joinAllocation.ToRelayServerData("dtls")
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException exception)
        {
            Debug.LogError(
                "Falscher Code oder Verbindung fehlgeschlagen: " +
                exception
            );

            codeEingabeFeld.interactable = true;
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {
        if (isStarting)
        {
            return;
        }

        bool hostCanStart =
            NetworkManager.Singleton.IsHost &&
            clientId != NetworkManager.Singleton.LocalClientId;

        bool clientCanStart =
            NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsHost &&
            clientId == NetworkManager.Singleton.LocalClientId;

        if (!hostCanStart && !clientCanStart)
        {
            return;
        }

        isStarting = true;
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
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

        float elapsedTime = 0f;

        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            if (loadingScreen != null)
            {
                loadingScreen.UpdateProgress(
                    Mathf.Clamp01(
                        elapsedTime / loadingDuration
                    )
                );
            }

            yield return null;
        }

        if (loadingScreen != null)
        {
            loadingScreen.UpdateProgress(1f);
        }

        yield return new WaitForSecondsRealtime(0.25f);

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
}