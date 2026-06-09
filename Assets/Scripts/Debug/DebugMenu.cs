using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    [Header("Wind")]
    public Button windToggle;
    public TextMeshProUGUI windToggleText;
    public Button windDirection;
    public Wind windScript;

    private void Start()
    {
        UpdateWindButtonText();
        windToggle.onClick.AddListener(windScript.ToggleWind);
        windToggle.onClick.AddListener(UpdateWindButtonText);

        windDirection.onClick.AddListener(SetWindDirection);
    }

    private void UpdateWindButtonText()
    {
        windToggleText.text = windScript.WindActive ? "ON" : "OFF";
    }

    private void SetWindDirection()
    {
        if(windScript.direction == Wind.Direction.LeftToRight)
        {
            windScript.SetDirection(Wind.Direction.RightToLeft);
        }
        else
        {
            windScript.SetDirection(Wind.Direction.LeftToRight);
        }
    }
    
}
