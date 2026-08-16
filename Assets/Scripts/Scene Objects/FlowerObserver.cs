using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class FlowerObserver : NetworkBehaviour
{
    public BoolStateObject[] flowers;
    private NetworkVariable<bool> allFlowersActive = new NetworkVariable<bool>(false);

    public UnityEvent OnAllFlowersActive;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }

        flowers = GetComponentsInChildren<BoolStateObject>();
        Debug.Log($"FlowerObserver found {flowers.Length} flowers.");
    }

    public void Check()
    {
        if (!IsServer) return;      

        if (flowers.Length == 0) return;

        foreach (BoolStateObject flower in flowers)
        {
            if (!flower.isActive.Value) return;
        }            

        if (allFlowersActive.Value) return;

        Debug.Log("All flowers are active!");
        allFlowersActive.Value = true;
        OnAllFlowersActive.Invoke();
        return;
    }

}
