using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaliGo.EditorTools
{
    /// <summary>
    /// Read-only measurement tool. Instantiates reference assets into an isolated preview
    /// scene (never the open scene), measures their renderer bounds, logs the results, then
    /// discards the preview scene. Makes no changes to any asset, scene object, or transform.
    /// </summary>
    [InitializeOnLoad]
    public static class MaliGoScaleAudit
    {
        const string CharacterPath = "Assets/kenney_animated-characters-protagonists/Model/characterMedium.fbx";
        const string SuburbanHousePath = "Assets/kenney_city-kit-suburban_20/Models/FBX format/building-type-a.fbx";
        const string CommercialBuildingPath = "Assets/kenney_city-kit-commercial_2.1/Models/FBX format/building-a.fbx";
        const string RoadStraightPath = "Assets/kenney_city-kit-roads/Models/FBX format/road-straight.fbx";
        const string FencePath = "Assets/kenney_city-kit-suburban_20/Models/FBX format/fence-1x2.fbx";
        const string SedanPath = "Assets/kenney_car-kit/Models/FBX format/sedan.fbx";

        static MaliGoScaleAudit()
        {
            // Skip during headless/batch builds - this spins up preview scenes and only
            // exists as a diagnostic, so it has no business running inside a build.
            if (Application.isBatchMode)
            {
                return;
            }

            EditorApplication.delayCall += RunAudit;
        }

        [MenuItem("MaliGo/Diagnostics/Scale Audit (Read-Only)")]
        public static void RunAudit()
        {
            Debug.Log("[ScaleAudit] ---- MaliGo scale audit (read-only, no scene/asset changes) ----");
            LogModelSize(CharacterPath, "Human PlayerCharacter model (characterMedium.fbx)");
            LogModelSize(SuburbanHousePath, "Player_House building (building-type-a.fbx)");
            LogModelSize(CommercialBuildingPath, "Local_Bank_Building (building-a.fbx)");
            LogModelSize(RoadStraightPath, "Road segment (road-straight.fbx)");
            LogModelSize(FencePath, "Fence panel, ~waist height reference (fence-1x2.fbx)");
            LogModelSize(SedanPath, "Car Kit sedan, placed unscaled - verify it matches City Kit (sedan.fbx)");
            Debug.Log("[ScaleAudit] ---- end audit ----");
        }

        static void LogModelSize(string assetPath, string label)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.Log($"[ScaleAudit] {label}: asset NOT FOUND at '{assetPath}'.");
                return;
            }

            Vector3? size = MeasureBoundsSize(prefab);
            if (size.HasValue)
            {
                Vector3 s = size.Value;
                Debug.Log($"[ScaleAudit] {label}: bounds size = (x:{s.x:0.###}, y(height):{s.y:0.###}, z:{s.z:0.###}) world units.");
            }
            else
            {
                Debug.Log($"[ScaleAudit] {label}: found asset but no Renderer to measure.");
            }
        }

        static Vector3? MeasureBoundsSize(GameObject prefab)
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject instance = Object.Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(instance, previewScene);
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    return null;
                }

                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                return bounds.size;
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }
    }
}
