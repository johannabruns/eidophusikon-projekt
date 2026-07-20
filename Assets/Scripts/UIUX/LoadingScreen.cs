using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Komponenten")]
    public GameObject loadingPanel; // Das gesamte Panel
    public Image backgroundImage;   // Das Bild, das den Text/Hintergrund zeigt
    public Image progressBarImage;  // Das Bild für den Stop-Motion Ladebalken

    [Header("Rotierende Artworks")]
    public Sprite[] randomBackgrounds; // Hier ziehst du alle Hintergrund-Bilder rein

    [Header("Stop-Motion Frames")]
    public Sprite[] loadingFrames; // Hier ziehst du die Ladebalken-Bilder rein (0% bis 100%)

    // Wird aufgerufen, wenn der Ladescreen gestartet wird
    public void ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);

        // 1. Zufälligen Hintergrund auswürfeln und setzen
        if (randomBackgrounds.Length > 0)
        {
            int randomIndex = Random.Range(0, randomBackgrounds.Length);
            backgroundImage.sprite = randomBackgrounds[randomIndex];
        }

        // 2. Ladebalken sicher auf das allererste Bild (0%) setzen
        if (loadingFrames.Length > 0)
        {
            progressBarImage.sprite = loadingFrames[0];
        }
    }

    // Diese Funktion füttern wir mit dem Ladefortschritt (ein Wert zwischen 0.0 und 1.0)
    public void UpdateProgress(float progress)
    {
        if (loadingFrames.Length == 0) return;

        // Mathe-Magie: Wir rechnen den Fortschritt in den passenden Array-Index um
        // Beispiel: 50% geladen (0.5) * 10 Bilder = Bild Nr. 5
        int frameIndex = Mathf.FloorToInt(progress * loadingFrames.Length);

        // Zur Sicherheit begrenzen, damit wir nicht über das Ziel hinausschießen (Error-Handling!)
        frameIndex = Mathf.Clamp(frameIndex, 0, loadingFrames.Length - 1);

        // Das berechnete Bild anzeigen
        progressBarImage.sprite = loadingFrames[frameIndex];
    }

    public void HideLoadingScreen()
    {
        loadingPanel.SetActive(false);
    }
}