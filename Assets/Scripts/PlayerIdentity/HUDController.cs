using MaliGo.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Binds the existing MaliGoWorld HUD to PlayerData at runtime.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Optional explicit references")]
        [SerializeField] Text playerNameText;
        [SerializeField] Text cashText;
        [SerializeField] Text savingsText;
        [SerializeField] Text financialStressText;
        [SerializeField] Text goalTitleText;
        [SerializeField] Text goalValueText;
        [SerializeField] RectTransform xpBarFill;

        void Awake()
        {
            AutoFindReferences();
        }

        void OnEnable()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnPlayerDataChanged += RefreshHUD;
                RefreshHUD(PlayerDataManager.Instance.CurrentPlayer);
            }
        }

        void OnDisable()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnPlayerDataChanged -= RefreshHUD;
            }
        }

        void Start()
        {
            if (PlayerDataManager.Instance != null)
            {
                RefreshHUD(PlayerDataManager.Instance.CurrentPlayer);
            }
        }

        void AutoFindReferences()
        {
            var texts = GetComponentsInChildren<Text>(true);

            foreach (var text in texts)
            {
                string value = text.text ?? string.Empty;

                if (playerNameText == null && value.StartsWith("Player:"))
                {
                    playerNameText = text;
                }
                else if (cashText == null && value.Contains("Cash:"))
                {
                    cashText = text;
                }
                else if (savingsText == null && value.Contains("Savings:"))
                {
                    savingsText = text;
                }
                else if (financialStressText == null && value.Contains("Financial Stress:"))
                {
                    financialStressText = text;
                }
                else if (goalTitleText == null && value.Contains("Today's Financial Goal"))
                {
                    goalTitleText = text;
                }
                else if (goalValueText == null && (value.Contains("Save R") || value.Contains("Goal:")))
                {
                    goalValueText = text;
                }
            }

            if (xpBarFill == null)
            {
                var fillTransform = transform.Find("HUD_StatusPanel/XP_Bar_Bg/XP_Bar_Fill");
                if (fillTransform == null)
                {
                    fillTransform = FindDeepChild(transform, "XP_Bar_Fill");
                }

                if (fillTransform != null)
                {
                    xpBarFill = fillTransform as RectTransform;
                }
            }
        }

        static Transform FindDeepChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                var nested = FindDeepChild(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        public void RefreshHUD(PlayerData data)
        {
            if (data == null)
            {
                return;
            }

            string displayName = string.IsNullOrWhiteSpace(data.characterName) ? "You" : data.characterName;
            int level = data.progression != null ? data.progression.level : 1;

            if (playerNameText != null)
            {
                playerNameText.text = $"Player: {displayName} | Level {level}";
            }

            if (data.financialStats != null)
            {
                if (cashText != null)
                {
                    cashText.text = $"💰 Cash: {FormatCurrency(data.financialStats.cash)}";
                }

                if (savingsText != null)
                {
                    savingsText.text = $"🏦 Savings: {FormatCurrency(data.financialStats.savings)}";
                }

                if (financialStressText != null)
                {
                    financialStressText.text = $"🌱 Financial Stress: {data.financialStats.financialStress:0}%";
                }
            }

            var goal = data.GetPrimaryGoal();
            if (goalTitleText != null)
            {
                goalTitleText.text = "Today's Financial Goal:";
            }

            if (goalValueText != null)
            {
                goalValueText.text = $"{goal.goalName}: Save {FormatCurrency(goal.dailyTarget)} today";
            }

            if (xpBarFill != null && data.progression != null)
            {
                float progress = data.progression.LevelProgressNormalized;
                xpBarFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
            }
        }

        static string FormatCurrency(float amount)
        {
            return $"R{amount:0,0}";
        }
    }
}
