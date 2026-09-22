using MaliGo.Characters;
using MaliGo.Data;
using MaliGo.PlayerIdentity;
using MaliGo.UI;
using UnityEngine;

namespace MaliGo.World
{
    /// <summary>
    /// Bank: makes the cash/savings split a deliberate, causal action rather than cosmetic -
    /// depositing/withdrawing/contributing always moves exactly the amount actually
    /// available (never credits more to one bucket than was debited from the other).
    /// </summary>
    public class BankInteraction : ProximityInteraction
    {
        ActionPanelUI panel;

        protected override void OnAwake()
        {
            SetPrompt("Press E to enter the bank");
            panel = gameObject.AddComponent<ActionPanelUI>();
        }

        protected override bool CanInteract()
        {
            return panel == null || !panel.IsOpen;
        }

        protected override void OnInteract()
        {
            panel.Show("Bank", BuildStatusText, new[]
            {
                new ActionPanelUI.ActionButton("Deposit R50", () => Deposit(50f)),
                new ActionPanelUI.ActionButton("Deposit R100", () => Deposit(100f)),
                new ActionPanelUI.ActionButton("Withdraw R50", () => Withdraw(50f)),
                new ActionPanelUI.ActionButton("Contribute R50 to goal", () => ContributeToGoal(50f))
            });
        }

        string BuildStatusText()
        {
            PlayerData data = PlayerDataAccess.GetCurrentPlayer();
            if (data?.financialStats == null)
            {
                return "No data yet.";
            }

            FinancialStats stats = data.financialStats;
            FinancialGoal goal = data.GetPrimaryGoal();

            return $"Cash: R{stats.cash:0}\n" +
                   $"Savings: R{stats.savings:0}\n" +
                   $"Goal - {goal.goalName}: R{goal.currentAmount:0} / R{goal.targetAmount:0}\n\n" +
                   "Deposits and withdrawals move exactly what you have available.";
        }

        void Deposit(float requestedAmount)
        {
            if (PlayerDataManager.Instance == null)
            {
                return;
            }

            float actual = 0f;
            PlayerDataManager.Instance.UpdatePlayerData(data =>
            {
                FinancialStats stats = data.financialStats;
                actual = Mathf.Min(stats.cash, requestedAmount);
                stats.cash -= actual;
                stats.savings += actual;
            }, saveImmediately: true);

            if (actual > 0f)
            {
                ShowMaliLine($"R{actual:0} moved into savings, {{0}}. That's protected from everyday spending now.");
            }
            else
            {
                ShowMaliLine("There's nothing in your cash to move right now, {0}.");
            }
        }

        void Withdraw(float requestedAmount)
        {
            if (PlayerDataManager.Instance == null)
            {
                return;
            }

            float actual = 0f;
            PlayerDataManager.Instance.UpdatePlayerData(data =>
            {
                FinancialStats stats = data.financialStats;
                actual = Mathf.Min(stats.savings, requestedAmount);
                stats.savings -= actual;
                stats.cash += actual;
            }, saveImmediately: true);

            if (actual > 0f)
            {
                ShowMaliLine($"R{actual:0} back into your cash, {{0}}. It's available again, for better or worse.");
            }
            else
            {
                ShowMaliLine("There's nothing in savings to draw from right now, {0}.");
            }
        }

        void ContributeToGoal(float requestedAmount)
        {
            if (PlayerDataManager.Instance == null)
            {
                return;
            }

            float actual = 0f;
            PlayerDataManager.Instance.UpdatePlayerData(data =>
            {
                FinancialStats stats = data.financialStats;
                actual = Mathf.Min(stats.cash, requestedAmount);
                stats.cash -= actual;
                data.GetPrimaryGoal().Deposit(actual);
            }, saveImmediately: true);

            if (actual > 0f)
            {
                ShowMaliLine($"R{actual:0} closer to your goal, {{0}}.");
            }
            else
            {
                ShowMaliLine("There's nothing in your cash to put toward that right now, {0}.");
            }
        }

        static void ShowMaliLine(string template)
        {
            GameObject maliObject = GameObject.Find("Mali");
            MaliDialogueController dialogue = maliObject != null ? maliObject.GetComponent<MaliDialogueController>() : null;
            dialogue?.ShowFormatted(template, PlayerDataAccess.GetCharacterName());
        }
    }
}
