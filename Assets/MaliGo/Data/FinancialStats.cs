using System;
using UnityEngine;

namespace MaliGo.Data
{
    [Serializable]
    public class FinancialStats
    {
        public float cash;
        public float savings;
        public float emergencyFund;
        public float financialStress;
        public float income;
        public float expenses;
        public float savingsRate;
        public float financialXP;

        public static FinancialStats CreateDefaults(LifeChapter chapter)
        {
            var stats = new FinancialStats();

            switch (chapter)
            {
                case LifeChapter.STUDENT:
                    stats.cash = 1500f;
                    stats.savings = 400f;
                    stats.emergencyFund = 200f;
                    stats.income = 2500f;
                    stats.expenses = 2200f;
                    stats.financialStress = 35f;
                    break;
                case LifeChapter.YOUNG_PROFESSIONAL:
                    stats.cash = 5000f;
                    stats.savings = 1200f;
                    stats.emergencyFund = 800f;
                    stats.income = 12000f;
                    stats.expenses = 9500f;
                    stats.financialStress = 25f;
                    break;
                case LifeChapter.ESTABLISHED_ADULT:
                    stats.cash = 8000f;
                    stats.savings = 15000f;
                    stats.emergencyFund = 6000f;
                    stats.income = 22000f;
                    stats.expenses = 17000f;
                    stats.financialStress = 18f;
                    break;
                case LifeChapter.WEALTH_BUILDER:
                    stats.cash = 15000f;
                    stats.savings = 45000f;
                    stats.emergencyFund = 12000f;
                    stats.income = 35000f;
                    stats.expenses = 22000f;
                    stats.financialStress = 12f;
                    break;
                case LifeChapter.FINANCIAL_INDEPENDENCE:
                    stats.cash = 25000f;
                    stats.savings = 120000f;
                    stats.emergencyFund = 30000f;
                    stats.income = 40000f;
                    stats.expenses = 18000f;
                    stats.financialStress = 8f;
                    break;
            }

            stats.savingsRate = stats.income > 0f
                ? Mathf.Clamp01((stats.income - stats.expenses) / stats.income) * 100f
                : 0f;
            stats.financialXP = 0f;

            return stats;
        }
    }
}
