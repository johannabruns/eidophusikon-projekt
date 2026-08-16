using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : NetworkBehaviour
{
    [Header("Wind")]
    public Button windToggle;
    public TextMeshProUGUI windToggleText;
    public Button windDirection;
    public Button increaseWind;
    public Button decreaseWind;
    public Wind windScript;

    [Header("Act Management")]
    public GameManager gameManager;
    public Button loadAct1;
    public Button loadAct2;
    public Button loadAct3;
    public Button loadAct4;

    // DebugMenu.cs
    [Header("Solo Testing")]
    public Button forceInitialSetup;


    private void Start()
    {
        loadAct1.onClick.AddListener(() => gameManager.LoadActRpc(1));
        loadAct2.onClick.AddListener(() => gameManager.LoadActRpc(2));
        loadAct3.onClick.AddListener(() => gameManager.LoadActRpc(3));
        loadAct4.onClick.AddListener(() => gameManager.LoadActRpc(4));

        forceInitialSetup.onClick.AddListener(() => gameManager.ForceInitialActSetupRpc());

        //Wind 
        if (windScript == null) return;

        UpdateWindButtonText();

        windToggle.onClick.AddListener(windScript.ToggleWindRpc);
        windDirection.onClick.AddListener(windScript.ToggleDirectionRpc);

        decreaseWind.onClick.AddListener(() => windScript.SetIntensityRpc(windScript.currentIntensity.Value - 1f));
        increaseWind.onClick.AddListener(() => windScript.SetIntensityRpc(windScript.currentIntensity.Value + 1f));
    }

    private void OnEnable()
    {
        if (windScript != null)
            windScript.WindActiveChanged += HandleWindActiveChanged;
    }

    private void OnDisable()
    {
        if (windScript != null)
            windScript.WindActiveChanged -= HandleWindActiveChanged;
    }

    private void HandleWindActiveChanged(bool previous, bool current)
    {
        UpdateWindButtonText();
    }

    private void UpdateWindButtonText()
    {
        if (windScript == null) return;

        windToggleText.text = windScript.WindActive ? "ON" : "OFF";
    }
}