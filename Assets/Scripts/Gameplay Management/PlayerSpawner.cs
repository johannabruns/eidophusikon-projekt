using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefabs")]
    public GameObject hostPlayerPrefab;
    public GameObject clientPlayerPrefab;

    [Header("Start Spawn Points")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;

    [Header("Finale")]
    public Transform mechanicFinaleSpawnPoint;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton
                .ConnectionApprovalCallback +=
                ApprovalCheck;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton
                .ConnectionApprovalCallback -=
                ApprovalCheck;
        }
    }

    private void ApprovalCheck(
        NetworkManager
            .ConnectionApprovalRequest request,
        NetworkManager
            .ConnectionApprovalResponse response
    )
    {
        bool isHost =
            request.ClientNetworkId ==
            NetworkManager.ServerClientId;

        GameObject prefabToSpawn =
            isHost
                ? hostPlayerPrefab
                : clientPlayerPrefab;

        Vector3 spawnPoint =
            isHost
                ? hostSpawnPoint.position
                : clientSpawnPoint.position;

        response.Approved = true;
        response.CreatePlayerObject = true;

        response.PlayerPrefabHash =
            prefabToSpawn
                .GetComponent<NetworkObject>()
                .PrefabIdHash;

        response.Position = spawnPoint;
        response.Rotation =
            Quaternion.identity;

        response.Pending = false;
    }

    public bool TeleportMechanicToFinale()
    {
        NetworkManager networkManager =
            NetworkManager.Singleton;

        if (networkManager == null ||
            !networkManager.IsServer ||
            mechanicFinaleSpawnPoint == null)
        {
            return false;
        }

        ulong mechanicClientId =
            NetworkManager.ServerClientId;

        if (!networkManager
                .ConnectedClients
                .TryGetValue(
                    mechanicClientId,
                    out NetworkClient mechanicClient
                ))
        {
            return false;
        }

        if (mechanicClient.PlayerObject ==
            null)
        {
            return false;
        }

        PlayerMovement movement =
            mechanicClient.PlayerObject
                .GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return false;
        }

        movement.TeleportToRpc(
            mechanicFinaleSpawnPoint.position
        );

        return true;
    }

    [ContextMenu(
        "Teleport Mechanic To Finale"
    )]
    private void TestFinaleTeleport()
    {
        TeleportMechanicToFinale();
    }
}