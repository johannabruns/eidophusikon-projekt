using UnityEngine;

[RequireComponent(typeof(Carryable))]
public class LightFilter : MonoBehaviour
{
    public TimeOfDay timeOfDay = TimeOfDay.Morning;

    [ColorUsage(true, true)]
    public Color lightColor = Color.white;
}