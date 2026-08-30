using MaliGo.Characters;
using MaliGo.Data;
using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Applies Kenney skin textures/materials based on PlayerData.appearance.
    /// </summary>
    public class KenneyAppearanceVisualProvider : IAppearanceVisualProvider
    {
        readonly PlayerCharacterCatalog catalog;

        public KenneyAppearanceVisualProvider(PlayerCharacterCatalog catalog)
        {
            this.catalog = catalog;
        }

        public void ApplyAppearance(AppearanceData appearance, PlayerCharacterVisualController visualController)
        {
            if (appearance == null || visualController == null || catalog == null)
            {
                return;
            }

            Material runtimeMaterial = catalog.CreateRuntimeSkinMaterial(appearance);
            if (runtimeMaterial != null)
            {
                visualController.SetSkinMaterial(runtimeMaterial);
            }
            else
            {
                Texture2D skin = catalog.ResolveSkin(appearance);
                visualController.SetSkinTexture(skin);
            }
        }
    }
}
