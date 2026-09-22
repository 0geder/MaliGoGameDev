using MaliGo.Characters;
using MaliGo.PlayerIdentity;
using UnityEngine;

namespace MaliGo.World
{
    /// <summary>
    /// Work: go to work, receive income, Mali reacts. No panel - it's an action, not a
    /// decision with trade-offs. A short real-time cooldown stands in for "once per day"
    /// until a real day cycle exists to gate it properly.
    /// </summary>
    public class WorkInteraction : ProximityInteraction
    {
        const float IncomeAmount = 150f;
        const float EnergyCost = 20f;
        const float CooldownSeconds = 30f;

        float nextAvailableTime;

        protected override void OnAwake()
        {
            SetPrompt("Press E to work");
        }

        protected override bool CanInteract()
        {
            return Time.time >= nextAvailableTime;
        }

        protected override void OnInteract()
        {
            if (PlayerDataManager.Instance == null)
            {
                return;
            }

            nextAvailableTime = Time.time + CooldownSeconds;

            PlayerDataManager.Instance.UpdatePlayerData(data =>
            {
                var stats = data.financialStats;
                stats.cash += IncomeAmount;
                stats.energy = Mathf.Clamp(stats.energy - EnergyCost, 0f, 100f);
                stats.financialXP += 5f;
            }, saveImmediately: true);

            GameObject maliObject = GameObject.Find("Mali");
            MaliDialogueController dialogue = maliObject != null ? maliObject.GetComponent<MaliDialogueController>() : null;
            dialogue?.ShowFormatted($"R{IncomeAmount:0} for the day's work, {{0}}. What you do with it is up to you.", PlayerDataAccess.GetCharacterName());
        }
    }
}
