using UnityEngine;
using System.Collections; // Wichtig für unser "Drehbuch" (Coroutine)

public class LightningEffect : MonoBehaviour
{
    [Header("Die Grafiken (Hier reinziehen!)")]
    public GameObject blitzBild;
    public GameObject blitzeinschlagBild;

    [Header("Timings (in Sekunden)")]
    public float warteBisEinschlag = 0.1f; // Wie lange ist NUR der kleine Blitz zu sehen?
    public float dauerZusammenSichtbar = 0.3f; // Wie lange sind beide zusammen zu sehen?

    private void Awake()
    {
        // Zur Sicherheit am Anfang beide unsichtbar machen
        if (blitzBild != null) blitzBild.SetActive(false);
        if (blitzeinschlagBild != null) blitzeinschlagBild.SetActive(false);
    }

    // Dieser Befehl wird gleich von der Glocke aufgerufen
    public void Strike()
    {
        // Startet das Drehbuch
        StartCoroutine(LightningSequence());
    }

    private IEnumerator LightningSequence()
    {
        // 1. Nur den kleinen Blitz einschalten
        if (blitzBild != null) blitzBild.SetActive(true);

        // Kurz warten...
        yield return new WaitForSeconds(warteBisEinschlag);

        // 2. Den großen Einschlag dazu schalten (kleiner Blitz bleibt an)
        if (blitzeinschlagBild != null) blitzeinschlagBild.SetActive(true);

        // Wieder kurz warten...
        yield return new WaitForSeconds(dauerZusammenSichtbar);

        // 3. Beide eiskalt wieder abschalten
        if (blitzBild != null) blitzBild.SetActive(false);
        if (blitzeinschlagBild != null) blitzeinschlagBild.SetActive(false);
    }
}