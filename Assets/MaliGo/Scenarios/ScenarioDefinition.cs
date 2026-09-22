using System;
using MaliGo.Data;
using UnityEngine;

namespace MaliGo.Scenarios
{
    /// <summary>
    /// A single reusable, data-driven financial decision scenario. New scenarios are
    /// added by creating more instances of this type (via ScenarioLibrary, or later as
    /// [CreateAssetMenu] assets in the Editor) - never by writing a new scenario class.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioDefinition", menuName = "MaliGo/Scenario Definition")]
    public class ScenarioDefinition : ScriptableObject
    {
        public string scenarioId = "";
        public string title = "";
        public string description = "";
        public string locationHint = "";

        [Tooltip("Empty = available regardless of the player's current Life Chapter.")]
        public LifeChapter[] requiredLifeChapters = Array.Empty<LifeChapter>();

        [Tooltip("Mali's line shown before the choices are presented. \"{0}\" is replaced with the player's name.")]
        public string introDialogue = "";

        public ScenarioChoice[] choices = Array.Empty<ScenarioChoice>();

        public bool IsAvailableForLifeChapter(LifeChapter chapter)
        {
            if (requiredLifeChapters == null || requiredLifeChapters.Length == 0)
            {
                return true;
            }

            foreach (var required in requiredLifeChapters)
            {
                if (required == chapter)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
