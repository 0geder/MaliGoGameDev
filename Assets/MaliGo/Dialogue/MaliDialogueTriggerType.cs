namespace MaliGo.Dialogue
{
    /// <summary>
    /// Categorizes why a MaliDialogueEntry would be shown. Only a subset is wired to a
    /// live trigger today (see MaliContextualDialogueSelector) - the rest exist so future
    /// systems (location entry, scenario hooks, chapter transitions) can add entries
    /// without changing this enum or Mali's controller.
    /// </summary>
    public enum MaliDialogueTriggerType
    {
        FirstMeeting,
        DefaultGreeting,
        EnterLocation,
        ScenarioBefore,
        ScenarioAfter,
        SavingsMilestone,
        FinancialStress,
        GoalProgress,
        ChapterTransition
    }
}
