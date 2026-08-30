using MaliGo.Characters;
using MaliGo.Data;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Pluggable interface for applying appearance data to the human player visual.
    /// </summary>
    public interface IAppearanceVisualProvider
    {
        void ApplyAppearance(AppearanceData appearance, PlayerCharacterVisualController visualController);
    }
}
