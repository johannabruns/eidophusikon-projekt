using Unity.Netcode;
using UnityEngine;

public class StageAnimationSequence : NetworkBehaviour
{
    public Animator animator;
    public AudioSource audioSource;

    public string animationTriggerName;
    public int playAfterQuest;

    public override void OnNetworkSpawn()
    {
        QuestManager.OnQuestComplete += CheckForQuest;
    }

    public override void OnNetworkDespawn()
    {
        QuestManager.OnQuestComplete -= CheckForQuest;
    }

    public void CheckForQuest(int index)
    {
        if (index != playAfterQuest) return;

        PlayAnimationRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayAnimationRpc()
    {
        animator.SetTrigger(animationTriggerName);
        audioSource.Play();
    }


}
