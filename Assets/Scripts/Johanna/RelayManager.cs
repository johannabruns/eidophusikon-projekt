using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using TMPro;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [Header("UI Basis")]
    public GameObject startMenuPanel;
    public GameObject buttonContainer; 
    
    [Header("Eingabe & Anzeige")]
    public TMP_InputField codeEingabeFeld; 
    public TextMeshProUGUI codeAnzeigeText; 

    [Header("Lade-Sequenz")]
    public TextMeshProUGUI statusText; 
    public Slider loadingBar; 

    // Das Schloss: Verhindert, dass das Drehbuch durch Netzwerk-Echos doppelt abläuft
    private bool isStarting = false; 

    async void Start()
    {
        statusText.gameObject.SetActive(false);
        if (loadingBar != null) loadingBar.gameObject.SetActive(false);

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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

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

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

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
        // Wenn das Drehbuch schon läuft, breche hier ab!
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
        // 1. Karton ausblenden (nimmt jetzt Eingabefeld und Texte automatisch mit!)
        if (buttonContainer != null) buttonContainer.SetActive(false);

        // 2. Lade-UI einschalten
        if (statusText != null) statusText.gameObject.SetActive(true);
        if (loadingBar != null) 
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.value = 0f; 
        }

        // 3. Den Balken panzern und elegant füllen
        float ladeDauer = 3.0f;
        float verstrichneZeit = 0f;

        while (verstrichneZeit < ladeDauer)
        {
            // Der Ruckler-Schutz: Niemals mehr als 0.1 Sekunden pro Frame aufschlagen
            verstrichneZeit += Mathf.Min(Time.deltaTime, 0.1f); 
            
            if (loadingBar != null)
            {
                loadingBar.value = verstrichneZeit / ladeDauer;
            }
            yield return null; 
        }

        // 4. Panel weg, Spiel starten!
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
    }
}