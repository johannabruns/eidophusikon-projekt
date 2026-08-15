using Unity.Netcode;
using UnityEngine;

public class Hedgehog : NetworkBehaviour
{
    public Animator anim;

    public NetworkVariable<bool> isHappy = new NetworkVariable<bool>(false);


    public override void OnNetworkSpawn()
    {
        isHappy.OnValueChanged += OnHappinessChanged;
    }

    public override void OnNetworkDespawn()
    {
        isHappy.OnValueChanged -= OnHappinessChanged;
    }

    private void OnHappinessChanged(bool previousValue, bool newValue)
    {
        anim.SetBool("isHappy", newValue);
    }

    public void SetHappy()
    {
        isHappy.Value = true;
    }
}
