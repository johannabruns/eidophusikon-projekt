using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    public Button joinAsHost;
    public Button joinAsClient;

    private void Start()
    {
        joinAsClient.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        joinAsHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
    }
}
