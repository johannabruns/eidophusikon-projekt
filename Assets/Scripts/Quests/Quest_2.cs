using UnityEngine;

public class Quest_2 : Quest
{
    public override TimeOfDay timeOfDay { get; protected set; } = TimeOfDay.Day;

    public override bool IsComplete()
    {
        Debug.Log("Quest 2 is complete!");
        return true;
    }
}
