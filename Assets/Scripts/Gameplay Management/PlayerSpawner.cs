using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject hostPlayerPrefab;
    public GameObject clientPlayerPrefab;
    [Space]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
    }


    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
                                NetworkManager.ConnectionApprovalResponse response)
    {
        bool isHost = request.ClientNetworkId == NetworkManager.ServerClientId;

        GameObject prefabToSpawn = isHost ? hostPlayerPrefab : clientPlayerPrefab;
        Vector3 spawnPoint = isHost ? hostSpawnPoint.position : clientSpawnPoint.position;

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = prefabToSpawn.GetComponent<NetworkObject>().PrefabIdHash;
        response.Position = spawnPoint;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }
}



