using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Loads Kenney player character assets at runtime, with Editor fallbacks before setup assets exist.
    /// </summary>
    public static class KenneyRuntimeCatalogFactory
    {
        const string KenneyRoot = "Assets/kenney_animated-characters-protagonists";
        const string ModelPath = KenneyRoot + "/Model/characterMedium.fbx";
        const string AnimatorPath = "Assets/MaliGo/Characters/PlayerCharacterAnimator.controller";
        const string MaterialPath = "Assets/MaliGo/Characters/PlayerSkinMaterial.mat";

        public static PlayerCharacterCatalog LoadCatalog()
        {
            PlayerCharacterCatalog catalog = Resources.Load<PlayerCharacterCatalog>("PlayerCharacterCatalog");
            if (catalog != null && catalog.characterModelPrefab != null)
            {
                return catalog;
            }

#if UNITY_EDITOR
            return CreateEditorFallbackCatalog();
#else
            return catalog;
#endif
        }

#if UNITY_EDITOR
        static PlayerCharacterCatalog CreateEditorFallbackCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<PlayerCharacterCatalog>();
            catalog.characterModelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            catalog.animatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);
            catalog.baseSkinMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            if (catalog.baseSkinMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                catalog.baseSkinMaterial = new Material(shader);
            }

            catalog.skinOptions = new[]
            {
                CreateSkinOption("skater_male", KenneyRoot + "/Skins/skaterMaleA.png"),
                CreateSkinOption("skater_female", KenneyRoot + "/Skins/skaterFemaleA.png"),
                CreateSkinOption("criminal_male", KenneyRoot + "/Skins/criminalMaleA.png"),
                CreateSkinOption("cyborg_female", KenneyRoot + "/Skins/cyborgFemaleA.png")
            };

            return catalog;
        }

        static PlayerCharacterCatalog.SkinOption CreateSkinOption(string id, string texturePath)
        {
            return new PlayerCharacterCatalog.SkinOption
            {
                optionId = id,
                skinTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath)
            };
        }
#endif
    }
}
