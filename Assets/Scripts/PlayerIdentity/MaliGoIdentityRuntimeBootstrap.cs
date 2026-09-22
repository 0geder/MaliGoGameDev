using MaliGo.Characters;
using MaliGo.PlayerIdentity;
using MaliGo.Scenarios;
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
            // AfterSceneLoad fires exactly once, for whichever scene the app boots into.
            // In a build that is CharacterCreation, so the world - which is reached later via
            // SceneManager.LoadScene - would never get wired: no player spawner, no mobile
            // controls, no scenario manager. Subscribing to sceneLoaded covers every scene
            // after the first. (In the Editor this was masked by pressing Play with
            // MaliGoWorld already open, which made the world the boot scene.)
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            WireScene(SceneManager.GetActiveScene().name);
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            WireScene(scene.name);
        }

        static void WireScene(string sceneName)
        {
            GameFlowController.EnsurePlayerDataManager();
            MaliGo.UI.EventSystemUtility.EnsureEventSystem();

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
            // Ordered by how badly the player is stranded without it, and each step isolated:
            // these all ran as one unguarded sequence before, so a throw anywhere above the
            // mobile controls left the tester with a world they could look at but not move in.
            Step("player spawner", () =>
            {
                if (Object.FindFirstObjectByType<PlayerCharacterSpawner>() == null)
                {
                    EnsureSystemsObject().AddComponent<PlayerCharacterSpawner>();
                }
            });

            Step("mobile controls", () =>
            {
                if (Object.FindFirstObjectByType<MaliGo.UI.MobileControlsUI>() == null)
                {
                    new GameObject("MobileControls").AddComponent<MaliGo.UI.MobileControlsUI>();
                }
            });

            Step("game flow", () =>
            {
                if (Object.FindFirstObjectByType<GameFlowController>() == null)
                {
                    EnsureSystemsObject().AddComponent<GameFlowController>();
                }
            });

            Step("HUD", () =>
            {
                var canvas = GameObject.Find("MaliGo_Canvas");
                if (canvas != null && canvas.GetComponent<HUDController>() == null)
                {
                    canvas.AddComponent<HUDController>();
                }
            });

            Step("scenarios", () =>
            {
                ScenarioWorldWiring.EnsureScenarioManager(EnsureSystemsObject());
                ScenarioWorldWiring.EnsureAllScenarioTriggers();
            });

            Step("world locations", MaliGo.World.WorldLocationWiring.EnsureLocations);
        }

        static GameObject EnsureSystemsObject()
        {
            return GameObject.Find("MaliGo_Systems") ?? new GameObject("MaliGo_Systems");
        }

        /// <summary>
        /// Runs one wiring step, keeping a failure local. There is no console on a test
        /// device, so the failure is logged in a form that shows up under `adb logcat`
        /// rather than vanishing.
        /// </summary>
        static void Step(string label, System.Action action)
        {
            try
            {
                action();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MaliGoBootstrap] World wiring step '{label}' failed, continuing with the rest: {ex}");
            }
        }

        static void WireCharacterCreationScene()
        {
            // A returning player with a valid save should never see character creation again -
            // without this, CharacterCreation as the boot scene would force it on every launch.
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsCharacterCreated)
            {
                GameFlowController.LoadWorldScene();
                return;
            }

            if (Object.FindFirstObjectByType<CharacterCreationUI>() == null)
            {
                var bootstrap = new GameObject("CharacterCreationBootstrap");
                bootstrap.AddComponent<CharacterCreationUI>();
            }
        }
    }
}
