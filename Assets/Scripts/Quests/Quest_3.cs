using UnityEngine;

public class Quest_3 : Quest
{
    public LeafPile leafPile;

    public override TimeOfDay timeOfDay { get; protected set; } = TimeOfDay.Evening;

    public override bool IsComplete()
    {
        return leafPile.IsComplete;
    }
}
