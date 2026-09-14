using Unity.Netcode;
using UnityEngine;

public class BeeSwarm : NetworkBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        QuestManager.OnQuestComplete += EnterStage;
    }

    public override void OnNetworkDespawn()
    {
        QuestManager.OnQuestComplete -= EnterStage;
    }

    public void EnterStage(int quest)
    {
        if (quest != 2) return;

        animator.SetTrigger("Enter");
        audioSource.Play();
    }
}
