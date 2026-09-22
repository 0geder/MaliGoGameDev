using System;

namespace MaliGo.Scenarios
{
    /// <summary>
    /// One selectable option within a ScenarioDefinition. Consequence fields are
    /// deltas applied to the player's FinancialStats when the choice is made.
    /// </summary>
    [Serializable]
    public class ScenarioChoice
    {
        public string choiceId = "";
        public string label = "";
        public string description = "";

        public float cashDelta;
        public float savingsDelta;
        public float financialStressDelta;
        public float energyDelta;
        public float financialXpDelta;

        public ScenarioBehaviourTag behaviourTag = ScenarioBehaviourTag.Neutral;

        /// <summary>Mali's line after this choice is made. "{0}" is replaced with the player's name.</summary>
        public string maliReactionLine = "";
    }
}
