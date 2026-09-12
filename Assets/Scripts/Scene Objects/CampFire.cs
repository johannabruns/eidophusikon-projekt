using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class CampFire : NetworkBehaviour
{
    public SpriteRenderer inactiveFire;
    public SpriteRenderer wood;
    public Sprite[] woodSprites;
    public GameObject fire;
    public SpriteRenderer[] stones;

    public AudioSource audioSource;

    private NetworkVariable<int> woodCount = new NetworkVariable<int>(-1);
    private NetworkVariable<int> stoneCount = new NetworkVariable<int>(-1);

    [HideInInspector]
    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    [HideInInspector]
    public NetworkVariable<bool> isLit = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        woodCount.OnValueChanged += OnWoodCountChanged;
        stoneCount.OnValueChanged += OnStoneCountChanged;
        isLit.OnValueChanged += OnIsLitChanged;
        isReady.OnValueChanged += OnIsReadyChanged;

        SetStoneSprite(stoneCount.Value);
        SetWoodSprite(woodCount.Value);
        SetFireState(isLit.Value);
    }

    public override void OnNetworkDespawn()
    {
        woodCount.OnValueChanged -= OnWoodCountChanged;
        stoneCount.OnValueChanged -= OnStoneCountChanged;
        isLit.OnValueChanged -= OnIsLitChanged;
        isReady.OnValueChanged -= OnIsReadyChanged;
    }

    public bool IsReady()
    {
        return woodCount.Value >= woodSprites.Length - 1 && stoneCount.Value >= stones.Length - 1;
    }

    private void OnIsReadyChanged(bool previousValue, bool newValue)
    {
        if (!IsServer) return;

        if (newValue)
        {
            LightFire();
        }
    }


    /*----------------------------Wood----------------------------*/

    [Rpc(SendTo.Server)]
    public void AddWoodRpc()
    {
        AddWood();
    }

    private void AddWood()
    {
        if (!IsServer) return;
        woodCount.Value++;

        isReady.Value = IsReady();
    }

    private void OnWoodCountChanged(int previousValue, int newValue)
    {
        SetWoodSprite(newValue);
    }

    private void SetWoodSprite(int index)
    {
        if (index >= woodSprites.Length)
            return;

        if (index < 0)
        {
            wood.sprite = null;
            return;
        }

        inactiveFire.enabled = index < 0;

        wood.sprite = woodSprites[index];
    }

    /*----------------------------Stone----------------------------*/

    [Rpc(SendTo.Server)]
    public void AddStoneRpc()
    {
        AddStone();
    }

    private void AddStone()
    {
        if (!IsServer) return;
        stoneCount.Value++;

        isReady.Value = IsReady();
    }

    private void OnStoneCountChanged(int previousValue, int newValue)
    {
        SetStoneSprite(newValue);
    }

    private void SetStoneSprite(int index)
    {
        if (index >= woodSprites.Length) return;

        for (int i = 0; i < stones.Length; i++)
        {
            stones[i].enabled = i <= index;
        }
    }

    /*----------------------------Flame----------------------------*/

    [Rpc(SendTo.Server)]
    public void LightFireRpc()
    {
        LightFire();
    }

    [Rpc(SendTo.Server)]
    public void ExtinguishFireFireRpc()
    {
        ExtinguishFire();
    }

    [Rpc(SendTo.Server)]
    public void ToggleFireRpc()
    {
        if (isLit.Value)
        {
            ExtinguishFire();
        }
        else
        {
            LightFire();
        }
    }

    private void LightFire()
    {
        if (!IsServer) return;

        if (!isReady.Value) return;

        isLit.Value = true;
        QuestManager.Instance.CheckQuestCompletion();
    }

    private void ExtinguishFire()
    {
        if (!IsServer) return;
        isLit.Value = false;
    }

    private void OnIsLitChanged(bool previousValue, bool newValue)
    {
        SetFireState(newValue);
    }

    private void SetFireState(bool state)
    {
        fire.SetActive(state);

        if (state)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}