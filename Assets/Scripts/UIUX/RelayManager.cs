using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;

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

    [Header("Nächster Schritt")]
    public GameObject zweiterCanvas;

    private bool isStarting = false; 

    async void Start()
    {
        // Ladescreen beim Start direkt unsichtbar machen
        if (loadingScreen != null) loadingScreen.HideLoadingScreen();

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
        }
    }

    public async void CreateRelayAndStartHost()
    {
        codeAnzeigeText.text = "Erstelle Server...";
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            codeAnzeigeText.text = "Code: " + joinCode;

           NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(allocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            codeAnzeigeText.text = "Fehler bei der Verbindung!";
            Debug.LogError(e);
        }
    }

    public async void JoinRelayAndStartClient()
    {
        codeEingabeFeld.interactable = false; 
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(codeEingabeFeld.text);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Falscher Code oder Verbindung fehlgeschlagen: " + e);
            codeEingabeFeld.interactable = true; 
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {
        if (isStarting) return; 

        if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            isStarting = true;
            StartCoroutine(StartGameSequence());
        }
        else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            isStarting = true;
            StartCoroutine(StartGameSequence());
        }
    }

    private IEnumerator StartGameSequence()
    {
        // 1. Eingabefelder ausblenden
        if (buttonContainer != null) buttonContainer.SetActive(false);
        
        // 2. Unseren Beta-Ladescreen aktivieren (würfelt Artwork und setzt Balken auf 0)
        if (loadingScreen != null) 
        {
            loadingScreen.ShowLoadingScreen();
        }

        // 3. Den Stop-Motion Balken füllen
        float ladeDauer = 3.0f;
        float verstrichneZeit = 0f;

        while (verstrichneZeit < ladeDauer)
        {
            verstrichneZeit += Mathf.Min(Time.deltaTime, 0.1f); 
            
            if (loadingScreen != null)
            {
                float aktuellerFortschritt = verstrichneZeit / ladeDauer;
                loadingScreen.UpdateProgress(aktuellerFortschritt);
            }
            yield return null; 
        }

        // 4. Panel weg, Spiel starten!
        if (loadingScreen != null) loadingScreen.HideLoadingScreen();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);

        // --- NEU: 5. Zweiten Canvas einblenden ---
        if (zweiterCanvas != null) zweiterCanvas.SetActive(true);
    }
    }
