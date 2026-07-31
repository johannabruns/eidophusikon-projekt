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
}

// GANZ WICHTIG: Das Skript erbt jetzt von NetworkBehaviour!
public class RopeController : NetworkBehaviour
{
    [Header("Die 3 Seilzüge")]
    public SeilSystem[] seilSysteme = new SeilSystem[3];

    [Header("Einstellungen Vertikal")]
    public float scrollGeschwindigkeit = 1.0f;
    public float minLaenge = 1.0f;
    public float maxLaenge = 10.0f;

    [Header("Einstellungen Horizontal")]
    public float horizontalesScrollTempo = 0.5f; 

    private int aktivesSeilIndex = -1;

    // Netzwerk-Variablen: Der Server speichert die echte Länge und teilt sie allen Clients mit
    public NetworkVariable<float> seil1Laenge = new NetworkVariable<float>(1f);
    public NetworkVariable<float> seil2Laenge = new NetworkVariable<float>(1f);
    public NetworkVariable<float> seil3Laenge = new NetworkVariable<float>(1f);

    public NetworkVariable<float> seil1Offset = new NetworkVariable<float>(0f);
    public NetworkVariable<float> seil2Offset = new NetworkVariable<float>(0f);
    public NetworkVariable<float> seil3Offset = new NetworkVariable<float>(0f);

    void Update()
    {
        // 1. Lokale Tastenabfrage
        if (Input.GetKeyDown(KeyCode.Alpha1)) aktivesSeilIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) aktivesSeilIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) aktivesSeilIndex = 2;

        // 2. Scroll-Eingabe ans Netzwerk senden
        if (aktivesSeilIndex != -1)
        {
            float scrollInput = Input.mouseScrollDelta.y;
            if (scrollInput != 0f)
            {
                // Statt es lokal zu berechnen, schicken wir den Befehl an den Server!
                UpdateRopeServerRpc(aktivesSeilIndex, scrollInput);
            }
        }

        // 3. Die synchronisierten Werte auf die Grafiken anwenden (für ALLE Spieler in Echtzeit)
        ApplyVisuals(0, seil1Laenge.Value, seil1Offset.Value);
        ApplyVisuals(1, seil2Laenge.Value, seil2Offset.Value);
        ApplyVisuals(2, seil3Laenge.Value, seil3Offset.Value);
    }

    // Dieser Befehl wird NUR auf dem Server ausgeführt.
    [Rpc(SendTo.Server)]
    public void UpdateRopeServerRpc(int index, float scrollInput)
    {
        float lengthChange = scrollInput * scrollGeschwindigkeit;
        float offsetChange = scrollInput * horizontalesScrollTempo;

        if (index == 0) 
        {
            seil1Laenge.Value = Mathf.Clamp(seil1Laenge.Value + lengthChange, minLaenge, maxLaenge);
            seil1Offset.Value += offsetChange;
        }
        else if (index == 1) 
        {
            seil2Laenge.Value = Mathf.Clamp(seil2Laenge.Value + lengthChange, minLaenge, maxLaenge);
            seil2Offset.Value += offsetChange;
        }
        else if (index == 2) 
        {
            seil3Laenge.Value = Mathf.Clamp(seil3Laenge.Value + lengthChange, minLaenge, maxLaenge);
            seil3Offset.Value += offsetChange;
        }
    }

    // Setzt die Grafiken für jeden Spieler exakt auf die Werte des Servers
    private void ApplyVisuals(int index, float laenge, float offset)
    {
        SeilSystem aktiv = seilSysteme[index];
        
        if (aktiv.vertikalesSeil != null)
        {
            Vector2 groesse = aktiv.vertikalesSeil.size;
            groesse.y = laenge;
            aktiv.vertikalesSeil.size = groesse;
            
            if (aktiv.seilEnde != null)
                aktiv.seilEnde.localPosition = new Vector3(0, -laenge, 0);
        }

        if (aktiv.horizontalesSeil != null)
        {
            Vector2 currentOffset = aktiv.horizontalesSeil.material.mainTextureOffset;
            currentOffset.y = offset;
            aktiv.horizontalesSeil.material.mainTextureOffset = currentOffset;
        }

        if (aktiv.buehnenSeil != null)
        {
            Vector2 groesse = aktiv.buehnenSeil.size;
            groesse.y = laenge;
            aktiv.buehnenSeil.size = groesse;

            if (aktiv.buehnenSeilEnde != null)
                aktiv.buehnenSeilEnde.localPosition = new Vector3(0, -laenge, 0);
        }
    }
}