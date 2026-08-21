using System;
using UnityEngine;

namespace MaliGo.Data
{
    [Serializable]
    public class FinancialGoal
    {
        public string goalId;
        public string goalName;
        public float targetAmount;
        public float currentAmount;
        public float dailyTarget;
        public string status;
        public string category;

        public float ProgressNormalized => targetAmount > 0 ? Mathf.Clamp01(currentAmount / targetAmount) : 0f;
        public float ProgressPercentage => ProgressNormalized * 100f;
        public float RemainingAmount => Mathf.Max(0, targetAmount - currentAmount);

        public FinancialGoal()
        {
            goalId = Guid.NewGuid().ToString();
            goalName = "Emergency Fund";
            targetAmount = 10000f;
            currentAmount = 3800f;
            dailyTarget = 200f;
            status = "In Progress";
            category = "Safety Net";
        }

        public FinancialGoal(string id, string name, float target, float current, float daily, string status = "In Progress", string category = "Savings")
        {
            goalId = id;
            goalName = name;
            targetAmount = target;
            currentAmount = current;
            dailyTarget = daily;
            this.status = status;
            this.category = category;
        }

        public void Deposit(float amount)
        {
            currentAmount = Mathf.Min(targetAmount, currentAmount + amount);
            if (currentAmount >= targetAmount)
            {
                status = "Completed";
            }
        }
    }
}
