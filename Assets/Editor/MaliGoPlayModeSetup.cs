using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Forces Play Mode to always start from CharacterCreation.unity, regardless of which
/// scene happens to be open in the Editor. Without this, having an unrelated scene open
/// (e.g. the blank URP2DSceneTemplate, or SampleScene) when pressing Play silently runs
/// that empty scene instead of the game - no error, just a blank screen, which is exactly
/// what was happening here. Re-applied on every script reload via [InitializeOnLoad] so
/// it survives Editor restarts rather than needing to be set manually each time.
/// </summary>
[InitializeOnLoad]
public static class MaliGoPlayModeSetup
{
    const string StartScenePath = "Assets/Scenes/CharacterCreation.unity";

    static MaliGoPlayModeSetup()
    {
        ApplyStartScene();
    }

    [MenuItem("MaliGo/Set Play Mode Start Scene")]
    public static void ApplyStartScene()
    {
        SceneAsset startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath);
        if (startScene == null)
        {
            Debug.LogWarning($"[MaliGoPlayModeSetup] Could not find {StartScenePath} - Play Mode start scene not set.");
            return;
        }

        EditorSceneManager.playModeStartScene = startScene;

        // Bonus tidy-up: disable the unused/abandoned scenes in Build Settings too
        // (SampleScene, MaliGoIsometricWorld) so only the two real scenes are checked.
        MaliGoBuildPipeline.SetShippingScenes();

        Debug.Log($"[MaliGoPlayModeSetup] Play Mode will always start from {StartScenePath} now, no matter which scene is open in the Editor.");
    }
}
