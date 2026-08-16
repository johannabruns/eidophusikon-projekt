using Unity.Netcode;
using UnityEngine;

/// <summary>
/// A wrapper class for managing the active state of a target GameObject in a networked environment.
/// Ensure that the targetObject is a child of the StageObjectWrapper
/// </summary>
public class StageObjectWrapper : NetworkBehaviour
{
    public GameObject targetObject;
    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(true);
    public bool spawnManually = false;

    public override void OnNetworkSpawn()
    {
        isActive.OnValueChanged += OnIsActiveChanged;
        targetObject.SetActive(isActive.Value); 
    }

    public override void OnNetworkDespawn()
    {
        isActive.OnValueChanged -= OnIsActiveChanged;
    }

    public void SetActive(bool active)
    {
        if (!IsServer) return;
        //Debug.Log($"Setting {targetObject.name} active state to {active}");
        isActive.Value = active;    
    }   

    private void OnIsActiveChanged(bool previousValue, bool newValue)
    {
        targetObject.SetActive(newValue);
    }


}
