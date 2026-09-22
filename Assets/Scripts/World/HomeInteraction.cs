using MaliGo.Characters;
using MaliGo.Data;
using MaliGo.PlayerIdentity;
using MaliGo.UI;
using UnityEngine;

namespace MaliGo.World
{
    /// <summary>
    /// Home: the player's base. View current finances and goal, and rest to recover energy.
    /// Reviewing recent decisions and ending the day are deferred until the day cycle exists.
    /// </summary>
    public class HomeInteraction : ProximityInteraction
    {
        ActionPanelUI panel;

        protected override void OnAwake()
        {
            SetPrompt("Press E to go inside");
            panel = gameObject.AddComponent<ActionPanelUI>();
        }

        protected override bool CanInteract()
        {
            return panel == null || !panel.IsOpen;
        }

        protected override void OnInteract()
        {
            panel.Show("Home", BuildStatusText, new[]
            {
                new ActionPanelUI.ActionButton("Rest (+30 energy)", Rest)
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
                   $"Financial Stress: {stats.financialStress:0}%\n" +
                   $"Energy: {stats.energy:0}%\n" +
                   $"Goal - {goal.goalName}: R{goal.currentAmount:0} / R{goal.targetAmount:0}";
        }

        void Rest()
        {
            if (PlayerDataManager.Instance == null)
            {
                return;
            }

            PlayerDataManager.Instance.UpdatePlayerData(data =>
            {
                data.financialStats.energy = Mathf.Clamp(data.financialStats.energy + 30f, 0f, 100f);
            }, saveImmediately: true);

            ShowMaliLine("A bit of rest goes a long way, {0}.");
        }

        static void ShowMaliLine(string template)
        {
            GameObject maliObject = GameObject.Find("Mali");
            MaliDialogueController dialogue = maliObject != null ? maliObject.GetComponent<MaliDialogueController>() : null;
            dialogue?.ShowFormatted(template, PlayerDataAccess.GetCharacterName());
        }
    }
}
