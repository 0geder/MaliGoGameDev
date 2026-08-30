using System;
using MaliGo.Data;
using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    [CreateAssetMenu(fileName = "PlayerCharacterCatalog", menuName = "MaliGo/Player Character Catalog")]
    public class PlayerCharacterCatalog : ScriptableObject
    {
        [Header("Kenney Assets")]
        public GameObject characterModelPrefab;
        public RuntimeAnimatorController animatorController;
        public Material baseSkinMaterial;

        [Header("Skin Options")]
        public SkinOption[] skinOptions = Array.Empty<SkinOption>();

        [Serializable]
        public struct SkinOption
        {
            public string optionId;
            public Texture2D skinTexture;
        }

        public Texture2D ResolveSkin(AppearanceData appearance)
        {
            if (appearance == null || skinOptions == null || skinOptions.Length == 0)
            {
                return null;
            }

            string requested = MapAppearanceToSkinId(appearance);

            foreach (SkinOption option in skinOptions)
            {
                if (option.optionId == requested)
                {
                    return option.skinTexture;
                }
            }

            return skinOptions[0].skinTexture;
        }

        public Material CreateRuntimeSkinMaterial(AppearanceData appearance)
        {
            if (baseSkinMaterial == null)
            {
                return null;
            }

            Material runtimeMaterial = new Material(baseSkinMaterial);
            Texture2D skin = ResolveSkin(appearance);
            if (skin != null)
            {
                if (runtimeMaterial.HasProperty("_BaseMap"))
                {
                    runtimeMaterial.SetTexture("_BaseMap", skin);
                }
                else if (runtimeMaterial.HasProperty("_MainTex"))
                {
                    runtimeMaterial.SetTexture("_MainTex", skin);
                }
            }

            return runtimeMaterial;
        }

        static string MapAppearanceToSkinId(AppearanceData appearance)
        {
            if (appearance.genderPresentation == "feminine")
            {
                return appearance.clothing == "smart" ? "cyborg_female" : "skater_female";
            }

            if (appearance.genderPresentation == "masculine")
            {
                return appearance.clothing == "smart" ? "criminal_male" : "skater_male";
            }

            return appearance.clothing == "sporty" ? "skater_male" : "skater_female";
        }
    }
}
