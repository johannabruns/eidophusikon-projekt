using UnityEngine;

public class Quest_2 : Quest
{
    public FlowerObserver flowerObserver;
    public override float nextQuestDelay { get; protected set; } = 3f;

    public override TimeOfDay timeOfDay { get; protected set; } = TimeOfDay.Day;

    public override bool IsComplete()
    {
        return flowerObserver.allFlowersActive.Value;
    }
}
