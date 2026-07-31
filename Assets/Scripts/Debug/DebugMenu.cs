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
    public Wind windScript;

    [Header("Act Management")]
    public GameManager gameManager;
    public Button loadAct1;
    public Button loadAct2;
    public Button loadAct3;
    public Button loadAct4;
    public Button loadAct5;

    private void Start()
    {
        loadAct1.onClick.AddListener(() => gameManager.LoadActRpc(1));
        loadAct2.onClick.AddListener(() => gameManager.LoadActRpc(2));
        loadAct3.onClick.AddListener(() => gameManager.LoadActRpc(3));
        loadAct4.onClick.AddListener(() => gameManager.LoadActRpc(4));
        loadAct5.onClick.AddListener(() => gameManager.LoadActRpc(5));

        //Wind 
        if (windScript == null) return;

        UpdateWindButtonText();

        windToggle.onClick.AddListener(ToggleWindRpc);
        windDirection.onClick.AddListener(SetWindDirectionRpc);
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

    [Rpc(SendTo.Server)]
    private void ToggleWindRpc()
    {
        if (windScript == null) return;

        windScript.ToggleWind();
    }

    [Rpc(SendTo.Server)]
    private void SetWindDirectionRpc()
    {
        if (windScript == null) return;

        if (windScript.Direction == Wind.WindDirection.LeftToRight)
            windScript.SetDirection(Wind.WindDirection.RightToLeft);
        else
            windScript.SetDirection(Wind.WindDirection.LeftToRight);
    }



}