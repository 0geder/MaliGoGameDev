using MaliGo.Characters;
using MaliGo.PlayerIdentity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Runtime wiring for identity systems when scenes have not yet been updated in the Editor.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class MaliGoIdentityRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootstrapAfterSceneLoad()
        {
            GameFlowController.EnsurePlayerDataManager();

            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "MaliGoWorld")
            {
                WireWorldScene();
            }
            else if (sceneName == "CharacterCreation")
            {
                WireCharacterCreationScene();
            }
        }

        static void WireWorldScene()
        {
            if (Object.FindFirstObjectByType<GameFlowController>() == null)
            {
                var systems = GameObject.Find("MaliGo_Systems") ?? new GameObject("MaliGo_Systems");
                systems.AddComponent<GameFlowController>();
            }

            var canvas = GameObject.Find("MaliGo_Canvas");
            if (canvas != null && canvas.GetComponent<HUDController>() == null)
            {
                canvas.AddComponent<HUDController>();
            }

            if (Object.FindFirstObjectByType<PlayerCharacterSpawner>() == null)
            {
                var systems = GameObject.Find("MaliGo_Systems") ?? new GameObject("MaliGo_Systems");
                systems.AddComponent<PlayerCharacterSpawner>();
            }
        }

        static void WireCharacterCreationScene()
        {
            if (Object.FindFirstObjectByType<CharacterCreationUI>() == null)
            {
                var bootstrap = new GameObject("CharacterCreationBootstrap");
                bootstrap.AddComponent<CharacterCreationUI>();
            }
        }
    }
}
