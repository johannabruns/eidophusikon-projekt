using Unity.Netcode;
using UnityEngine;

public abstract class Quest: NetworkBehaviour
{
    public abstract float nextQuestDelay { get; protected set; }
    public abstract TimeOfDay timeOfDay { get; protected set; }

    public abstract bool IsComplete();
}