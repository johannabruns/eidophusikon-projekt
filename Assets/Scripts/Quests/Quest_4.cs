using System;
using UnityEngine;

public class Quest_4 : Quest
{
    public ConstellationManager constellationManager;
    public CampFire campFire;

    public override TimeOfDay timeOfDay { get; protected set; } = TimeOfDay.Night;

    public override bool IsComplete()
    {
        return constellationManager.isAligned.Value && campFire.isLit.Value;
    }
}
