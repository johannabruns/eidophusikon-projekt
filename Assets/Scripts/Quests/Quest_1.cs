using UnityEngine;

public class Quest_1 : Quest
{
    public override TimeOfDay timeOfDay
    {
        get;
        protected set;
    } = TimeOfDay.Morning;

    public override float nextQuestDelay
    {
        get;
        protected set;
    } = 0f;

    [Header("Questziele")]
    public Birdcage birdcage;
    public RopeController ropeController;

    [Header("Debug")]
    [SerializeField]
    private bool zielErreicht;

    private void Update()
    {
        zielErreicht =
            CalculateCompletion();
    }

    public override bool IsComplete()
    {
        return CalculateCompletion();
    }

    private bool CalculateCompletion()
    {
        if (birdcage == null ||
            ropeController == null)
        {
            return false;
        }

        return
            birdcage.isOpen.Value &&
            ropeController
                .AlleBuehnenSeileImZiel;
    }
}