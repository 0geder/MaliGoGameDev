using System.Collections.Generic;
using MaliGo.Data;

namespace MaliGo.Dialogue
{
    /// <summary>
    /// Seeds MaliDialogueEntry instances in code, the same way ScenarioLibrary seeds
    /// scenarios - avoids hand-authoring fragile ScriptableObject assets for a first
    /// pass while keeping the data shape (MaliDialogueEntry) reusable and expandable.
    /// </summary>
    public static class MaliDialogueLibrary
    {
        static List<MaliDialogueEntry> entries;

        public static IReadOnlyList<MaliDialogueEntry> AllEntries
        {
            get
            {
                EnsureBuilt();
                return entries;
            }
        }

        public static MaliDialogueEntry FindById(string dialogueId)
        {
            EnsureBuilt();
            foreach (var entry in entries)
            {
                if (entry.dialogueId == dialogueId)
                {
                    return entry;
                }
            }

            return null;
        }

        static void EnsureBuilt()
        {
            if (entries != null)
            {
                return;
            }

            entries = new List<MaliDialogueEntry>
            {
                new MaliDialogueEntry
                {
                    dialogueId = "first_meeting",
                    triggerType = MaliDialogueTriggerType.FirstMeeting,
                    line = "Hey, {0}. I'm Mali. I'll be with you while you figure this whole money thing out."
                },
                new MaliDialogueEntry
                {
                    dialogueId = "financial_stress_high",
                    triggerType = MaliDialogueTriggerType.FinancialStress,
                    line = "Looks like things are getting tight, {0}. Before we spend anything else, let's check what you've got left."
                },
                new MaliDialogueEntry
                {
                    dialogueId = "low_cash",
                    triggerType = MaliDialogueTriggerType.FinancialStress,
                    line = "Things are looking a little thin right now, {0}. Might be worth going easy until more comes in."
                },
                new MaliDialogueEntry
                {
                    dialogueId = "repeated_discretionary_spending",
                    triggerType = MaliDialogueTriggerType.GoalProgress,
                    line = "You've made a few choices like that lately, {0}. Want to see what they're doing to your goal?"
                },
                new MaliDialogueEntry
                {
                    dialogueId = "savings_milestone",
                    triggerType = MaliDialogueTriggerType.SavingsMilestone,
                    line = "Look at that, {0}. Your savings are starting to become a habit."
                },
                new MaliDialogueEntry
                {
                    dialogueId = "consistent_saving",
                    triggerType = MaliDialogueTriggerType.SavingsMilestone,
                    line = "Nice, {0}. You actually chose your future over the impulse."
                },
                new MaliDialogueEntry
                {
                    dialogueId = "default_greeting_student",
                    triggerType = MaliDialogueTriggerType.DefaultGreeting,
                    applicableLifeChapters = new[] { LifeChapter.STUDENT },
                    line = "Let's start small, {0}. Where does your money usually disappear to?"
                },
                new MaliDialogueEntry
                {
                    dialogueId = "default_greeting",
                    triggerType = MaliDialogueTriggerType.DefaultGreeting,
                    line = "Hey, {0}! Ready to see what today has in store?"
                }
            };
        }
    }
}
