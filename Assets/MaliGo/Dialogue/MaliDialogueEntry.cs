using System;
using MaliGo.Data;
using UnityEngine;

namespace MaliGo.Dialogue
{
    /// <summary>
    /// One reusable, data-driven Mali line. New dialogue is added by creating more
    /// entries in MaliDialogueLibrary - never by writing new conditionals into
    /// MaliDialogueController or MaliNpcController.
    /// </summary>
    [Serializable]
    public class MaliDialogueEntry
    {
        public string dialogueId = "";
        public MaliDialogueTriggerType triggerType = MaliDialogueTriggerType.DefaultGreeting;

        [Tooltip("Empty = available regardless of the player's current Life Chapter.")]
        public LifeChapter[] applicableLifeChapters = Array.Empty<LifeChapter>();

        public float minFinancialXP;

        /// <summary>Mali's line. "{0}" is replaced with the player's name.</summary>
        public string line = "";

        public bool IsAvailableForLifeChapter(LifeChapter chapter)
        {
            if (applicableLifeChapters == null || applicableLifeChapters.Length == 0)
            {
                return true;
            }

            foreach (var chapterOption in applicableLifeChapters)
            {
                if (chapterOption == chapter)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
