using UnityEngine;

public class Quest_1 : Quest
{
    public override TimeOfDay timeOfDay { get; protected set; } = TimeOfDay.Morning;
    
    public Birdcage birdcage;

    public override bool IsComplete()
    {
        //TODO: Vögel

        return birdcage.isOpen.Value;
    }
}
