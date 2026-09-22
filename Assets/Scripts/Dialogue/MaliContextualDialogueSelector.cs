using MaliGo.Data;

namespace MaliGo.Dialogue
{
    /// <summary>
    /// Picks the single most relevant MaliDialogueEntry for the player's current
    /// PlayerData. Evaluated in priority order (most urgent first); the first
    /// matching condition wins. Falls back to a Life-Chapter-aware default greeting.
    /// </summary>
    public static class MaliContextualDialogueSelector
    {
        public static MaliDialogueEntry SelectLine(PlayerData data)
        {
            if (data == null)
            {
                return MaliDialogueLibrary.FindById("default_greeting");
            }

            if (!data.hasMetMali)
            {
                return MaliDialogueLibrary.FindById("first_meeting");
            }

            FinancialStats stats = data.financialStats;
            if (stats != null)
            {
                if (stats.financialStress >= 60f)
                {
                    return MaliDialogueLibrary.FindById("financial_stress_high");
                }

                if (stats.income > 0f && stats.cash < stats.income * 0.15f)
                {
                    return MaliDialogueLibrary.FindById("low_cash");
                }

                if (stats.spendingBehaviourScore > 0.6f)
                {
                    return MaliDialogueLibrary.FindById("repeated_discretionary_spending");
                }

                FinancialGoal goal = data.GetPrimaryGoal();
                if (goal != null && goal.targetAmount > 0f && stats.savings >= goal.targetAmount * 0.5f)
                {
                    return MaliDialogueLibrary.FindById("savings_milestone");
                }

                if (stats.savingBehaviourScore > 0.6f)
                {
                    return MaliDialogueLibrary.FindById("consistent_saving");
                }
            }

            return SelectDefaultGreeting(data.currentLifeChapter);
        }

        static MaliDialogueEntry SelectDefaultGreeting(LifeChapter chapter)
        {
            MaliDialogueEntry universalFallback = null;

            foreach (var entry in MaliDialogueLibrary.AllEntries)
            {
                if (entry.triggerType != MaliDialogueTriggerType.DefaultGreeting)
                {
                    continue;
                }

                if (entry.applicableLifeChapters.Length == 0)
                {
                    universalFallback = entry;
                    continue;
                }

                if (entry.IsAvailableForLifeChapter(chapter))
                {
                    return entry;
                }
            }

            return universalFallback;
        }
    }
}
