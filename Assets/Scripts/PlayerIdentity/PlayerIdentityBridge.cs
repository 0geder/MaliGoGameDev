using MaliGo.Characters;
using MaliGo.Data;
using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Connects PlayerData to the human PlayerCharacter visual representation.
    /// </summary>
    public class PlayerIdentityBridge : MonoBehaviour
    {
        [SerializeField] PlayerCharacterVisualController visualController;
        [SerializeField] PlayerCharacterCatalog catalog;

        IAppearanceVisualProvider appearanceProvider;

        void Awake()
        {
            if (visualController == null)
            {
                visualController = GetComponentInChildren<PlayerCharacterVisualController>();
            }

            if (catalog == null)
            {
                catalog = Resources.Load<PlayerCharacterCatalog>("PlayerCharacterCatalog");
            }

            appearanceProvider = catalog != null
                ? new KenneyAppearanceVisualProvider(catalog)
                : null;
        }

        void OnEnable()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnPlayerDataChanged += HandlePlayerDataChanged;
                ApplyCurrentPlayerData(PlayerDataManager.Instance.CurrentPlayer);
            }
        }

        void OnDisable()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnPlayerDataChanged -= HandlePlayerDataChanged;
            }
        }

        void Start()
        {
            if (PlayerDataManager.Instance != null)
            {
                ApplyCurrentPlayerData(PlayerDataManager.Instance.CurrentPlayer);
            }
        }

        public void Configure(PlayerCharacterVisualController visual, PlayerCharacterCatalog characterCatalog)
        {
            visualController = visual;
            catalog = characterCatalog;
            appearanceProvider = catalog != null
                ? new KenneyAppearanceVisualProvider(catalog)
                : null;
        }

        void HandlePlayerDataChanged(PlayerData data)
        {
            ApplyCurrentPlayerData(data);
        }

        public void ApplyCurrentPlayerData(PlayerData data)
        {
            if (data == null || visualController == null)
            {
                return;
            }

            appearanceProvider?.ApplyAppearance(data.appearance, visualController);

            if (!string.IsNullOrWhiteSpace(data.characterName))
            {
                gameObject.name = $"Player_{SanitizeName(data.characterName)}";
            }
        }

        static string SanitizeName(string name)
        {
            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }
    }
}
