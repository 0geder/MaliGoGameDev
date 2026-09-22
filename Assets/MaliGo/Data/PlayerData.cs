using System;

namespace MaliGo.Data
{
    [Serializable]
    public class PlayerData
    {
        public string characterName = "";
        public AppearanceData appearance = new AppearanceData();
        public FinancialProfile financialProfile = new FinancialProfile();
        public LifeChapter currentLifeChapter = LifeChapter.YOUNG_PROFESSIONAL;
        public FinancialStats financialStats = new FinancialStats();
        public FinancialGoal[] goals = Array.Empty<FinancialGoal>();
        public ProgressionData progression = new ProgressionData();
        public bool isCharacterCreated;
        public string[] completedScenarioIds = Array.Empty<string>();
        public bool hasMetMali;

        public static PlayerData CreateNew()
        {
            var data = new PlayerData
            {
                characterName = "",
                appearance = new AppearanceData(),
                financialProfile = new FinancialProfile(),
                currentLifeChapter = LifeChapter.YOUNG_PROFESSIONAL,
                financialStats = FinancialStats.CreateDefaults(LifeChapter.YOUNG_PROFESSIONAL),
                goals = new[] { new FinancialGoal() },
                progression = new ProgressionData(),
                isCharacterCreated = false
            };
            return data;
        }

        public FinancialGoal GetPrimaryGoal()
        {
            if (goals == null || goals.Length == 0)
            {
                return new FinancialGoal();
            }

            return goals[0];
        }
    }
}
