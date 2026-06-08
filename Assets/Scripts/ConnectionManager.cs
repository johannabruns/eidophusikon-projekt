using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Start()
    {
        // Wenn der User Host klickt, starte Server + Client und verstecke die Buttons
        hostButton.onClick.AddListener(() => {
            networkManager.StartHost();
            HideButtons();
        });
        
        // Wenn der User Client klickt, verbinde mit Server und verstecke die Buttons
        clientButton.onClick.AddListener(() => {
            networkManager.StartClient();
            HideButtons();
        });
    }

    // Diese neue Funktion schaltet die Buttons unsichtbar
    private void HideButtons()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}