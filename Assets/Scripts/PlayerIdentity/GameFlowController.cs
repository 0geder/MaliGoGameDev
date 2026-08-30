using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Ensures PlayerDataManager exists and routes players without a saved character
    /// to the character creation flow.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        const string CharacterCreationSceneName = "CharacterCreation";
        const string WorldSceneName = "MaliGoWorld";

        [SerializeField] bool requireCharacterCreation = true;

        void Awake()
        {
            EnsurePlayerDataManager();
        }

        void Start()
        {
            string activeScene = SceneManager.GetActiveScene().name;

            if (activeScene == WorldSceneName && requireCharacterCreation)
            {
                if (PlayerDataManager.Instance != null && !PlayerDataManager.Instance.IsCharacterCreated)
                {
                    SceneManager.LoadScene(CharacterCreationSceneName);
                }
            }
        }

        public static void EnsurePlayerDataManager()
        {
            if (PlayerDataManager.Instance != null)
            {
                return;
            }

            var existing = FindFirstObjectByType<PlayerDataManager>();
            if (existing != null)
            {
                return;
            }

            var go = new GameObject("PlayerDataManager");
            go.AddComponent<PlayerDataManager>();
        }

        public static void LoadWorldScene()
        {
            EnsurePlayerDataManager();
            SceneManager.LoadScene(WorldSceneName);
        }
    }
}
