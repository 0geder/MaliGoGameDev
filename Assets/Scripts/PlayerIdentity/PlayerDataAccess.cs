using MaliGo.Data;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Convenience accessors for gameplay systems that need player identity data.
    /// </summary>
    public static class PlayerDataAccess
    {
        public static PlayerData GetCurrentPlayer()
        {
            return PlayerDataManager.Instance != null
                ? PlayerDataManager.Instance.CurrentPlayer
                : null;
        }

        public static string GetCharacterName()
        {
            var player = GetCurrentPlayer();
            return player != null && !string.IsNullOrWhiteSpace(player.characterName)
                ? player.characterName
                : "You";
        }

        public static FinancialStats GetFinancialStats()
        {
            return GetCurrentPlayer()?.financialStats;
        }

        public static FinancialProfile GetFinancialProfile()
        {
            return GetCurrentPlayer()?.financialProfile;
        }

        public static LifeChapter GetLifeChapter()
        {
            var player = GetCurrentPlayer();
            return player != null ? player.currentLifeChapter : LifeChapter.YOUNG_PROFESSIONAL;
        }

        public static InternalArchetype GetInternalArchetype()
        {
            var profile = GetFinancialProfile();
            return profile != null ? profile.internalArchetype : InternalArchetype.Unassigned;
        }
    }
}
