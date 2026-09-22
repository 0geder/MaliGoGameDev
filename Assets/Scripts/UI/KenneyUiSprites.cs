using UnityEngine;

namespace MaliGo.UI
{
    /// <summary>
    /// Loads panel/button art from the Kenney UI Adventure pack for the runtime-built UI
    /// (HUD, Mali dialogue, scenario choices, Home/Bank panels).
    ///
    /// Loads from Resources first so it works in an actual player build - MaliGoResourceBaker
    /// copies the needed PNGs into Assets/Resources/MaliGoUI. Falls back to AssetDatabase in
    /// the Editor if the bake hasn't been run yet. The pack ships with no 9-slice border
    /// configured, so sprites are constructed here with an explicit border instead of
    /// overwriting each PNG's .meta file.
    /// </summary>
    public static class KenneyUiSprites
    {
        const string ResourcePath = "MaliGoUI/";
        const string EditorBasePath = "Assets/kenney_ui-pack-adventure/PNG/Default/";

        public static Sprite PanelWarm => LoadSliced("panel_brown", new Vector4(14, 14, 14, 14));
        public static Sprite PanelStatus => LoadSliced("panel_grey_green", new Vector4(12, 12, 12, 12));
        public static Sprite ButtonWarm => LoadSliced("button_brown", new Vector4(8, 6, 8, 6));
        public static Sprite BannerModern => LoadSliced("banner_modern", new Vector4(48, 10, 48, 10));
        public static Sprite ProgressBarFrame => LoadSliced("panel_border_brown", new Vector4(14, 14, 14, 14));

        static Sprite LoadSliced(string fileNameWithoutExtension, Vector4 border)
        {
            Texture2D texture = Resources.Load<Texture2D>(ResourcePath + fileNameWithoutExtension);

#if UNITY_EDITOR
            if (texture == null)
            {
                texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
                    EditorBasePath + fileNameWithoutExtension + ".png");
            }
#endif

            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border);
        }
    }
}
