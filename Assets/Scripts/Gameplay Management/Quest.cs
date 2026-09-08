using Unity.Netcode;
using UnityEngine;

public abstract class Quest: NetworkBehaviour
{
    public abstract TimeOfDay timeOfDay { get; protected set; }

    public abstract bool IsComplete();
}