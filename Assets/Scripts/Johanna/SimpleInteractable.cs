using UnityEngine;
using Unity.Netcode; 

public class SimpleInteractable : NetworkBehaviour
{
    [Header("UI")]
    public GameObject pressFPrompt; 

    [Header("Magie-Zuweisung (Nur EINS ankreuzen!)")]
    public bool isEarthquakeLever = false;
    public bool isWindWheel = false;
    public bool isBell = false;

    [Header("Hardware-Kabel (Nur das passende ausfüllen!)")]
    public WindEffect zielWindObjekt;
    public LightningEffect zielBlitzObjekt;
    public AudioSource earthquakeSound; // NEU: Das Kabel für das Rumpeln

    private bool isPlayerInRange = false;

    void Start()
    {
        if (pressFPrompt != null) pressFPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (isEarthquakeLever) TriggerEarthquakeServerRpc();
            if (isWindWheel) TriggerWindServerRpc();
            if (isBell) TriggerBellServerRpc(); // NEU: Glocke rufen
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerEarthquakeServerRpc() { TriggerEarthquakeClientRpc(); }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerWindServerRpc() { TriggerWindClientRpc(); }

    // NEU: ServerRpc für die Glocke
    [ServerRpc(RequireOwnership = false)]
    private void TriggerBellServerRpc() { TriggerBellClientRpc(); }

    [ClientRpc]
    private void TriggerWindClientRpc()
    {
        // if (IsServer) return; // (Kannst du später wieder einkommentieren)
        if (zielWindObjekt != null) zielWindObjekt.BlowWind();
    }

   [ClientRpc]
    private void TriggerEarthquakeClientRpc()
    {
        // TÜRSTEHER WIEDER AKTIV: Der Host (Maschinist) wackelt nicht mehr mit!
        if (IsServer) return; 
        
        if (CameraShaker.Instance != null) 
        {
            CameraShaker.Instance.Shake(1.5f, 0.05f); 
        }

        // Sound abspielen
        if (earthquakeSound != null)
        {
            earthquakeSound.Play();
            Invoke("StopEarthquakeSound", 1.5f); 
        }
    }

    // NEU: Die Methode, die vom Wecker gerufen wird
    private void StopEarthquakeSound()
    {
        if (earthquakeSound != null)
        {
            earthquakeSound.Stop();
        }
    }

    // NEU: ClientRpc für den Blitz
    [ClientRpc]
    private void TriggerBellClientRpc()
    {
        // Türsteher ist wieder aus, damit du es als Host testen kannst
        // if (IsServer) return; 

        if (zielBlitzObjekt != null)
        {
            zielBlitzObjekt.Strike();
        }
        else
        {
            Debug.Log("FEHLER: Kabel nicht verbunden! Zieh das Gewitter-Objekt in den Inspector der Glocke!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                isPlayerInRange = true;
                if (pressFPrompt != null) pressFPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                isPlayerInRange = false;
                if (pressFPrompt != null) pressFPrompt.SetActive(false);
            }
        }
    }
}