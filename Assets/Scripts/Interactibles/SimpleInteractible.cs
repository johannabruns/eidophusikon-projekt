using UnityEngine;

/// <summary>
/// An interactible that only responds to a single button press, such as a lever or a button.
/// </summary>
public abstract class SimpleInteractible : Interactible
{
    public abstract void OnInteract();
}
