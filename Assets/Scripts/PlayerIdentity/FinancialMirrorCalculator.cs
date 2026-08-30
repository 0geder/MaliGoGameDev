using MaliGo.Data;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Maps Financial Mirror questionnaire answers to an internal financial profile.
    /// Archetype assignments are for internal game systems only.
    /// </summary>
    public static class FinancialMirrorCalculator
    {
        public static FinancialProfile Calculate(FinancialMirrorAnswers answers)
        {
            var profile = new FinancialProfile
            {
                incomeType = answers.incomeType,
                spendingBehaviour = answers.spendingBehaviour,
                savingBehaviour = answers.savingBehaviour,
                primaryGoal = answers.primaryGoal,
                financialConfidence = answers.financialConfidence,
                riskTolerance = answers.riskTolerance,
                primaryChallenge = answers.primaryChallenge,
                internalArchetype = DetermineInternalArchetype(answers)
            };

            return profile;
        }

        public static LifeChapter DetermineLifeChapter(FinancialMirrorAnswers answers)
        {
            return answers.lifeChapter;
        }

        static InternalArchetype DetermineInternalArchetype(FinancialMirrorAnswers answers)
        {
            if (answers.lifeChapter == LifeChapter.STUDENT)
            {
                if (answers.savingBehaviour == "rarely" || answers.spendingBehaviour == "impulsive")
                {
                    return InternalArchetype.Thandi;
                }

                return InternalArchetype.Dinda;
            }

            if (answers.lifeChapter == LifeChapter.YOUNG_PROFESSIONAL)
            {
                if (answers.primaryGoal == "debt" || answers.primaryChallenge == "debt")
                {
                    return InternalArchetype.Alex;
                }

                return InternalArchetype.Sam;
            }

            if (answers.lifeChapter == LifeChapter.ESTABLISHED_ADULT)
            {
                if (answers.riskTolerance == "conservative" || answers.savingBehaviour == "consistent")
                {
                    return InternalArchetype.Dinda;
                }

                return InternalArchetype.Thandi;
            }

            if (answers.lifeChapter == LifeChapter.WEALTH_BUILDER)
            {
                return InternalArchetype.Alex;
            }

            return InternalArchetype.Sam;
        }
    }

    [System.Serializable]
    public class FinancialMirrorAnswers
    {
        public LifeChapter lifeChapter = LifeChapter.YOUNG_PROFESSIONAL;
        public string incomeType = "salary";
        public string spendingBehaviour = "balanced";
        public string savingBehaviour = "sometimes";
        public string primaryGoal = "emergency_fund";
        public int financialConfidence = 3;
        public string riskTolerance = "moderate";
        public string primaryChallenge = "budgeting";
    }
}
