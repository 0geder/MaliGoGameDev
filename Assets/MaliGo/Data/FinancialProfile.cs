using System;

namespace MaliGo.Data
{
    /// <summary>
    /// Internal archetype classifications used for scenarios and dialogue.
    /// Never display these names to the player.
    /// </summary>
    public enum InternalArchetype
    {
        Unassigned,
        Thandi,
        Alex,
        Dinda,
        Sam
    }

    [Serializable]
    public class FinancialProfile
    {
        public string incomeType = "";
        public string spendingBehaviour = "";
        public string savingBehaviour = "";
        public string primaryGoal = "";
        public int financialConfidence = 3;
        public string riskTolerance = "";
        public string primaryChallenge = "";
        public InternalArchetype internalArchetype = InternalArchetype.Unassigned;
    }
}
