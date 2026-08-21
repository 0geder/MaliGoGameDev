using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MaliGo.Environment.EditorTools
{
    /// <summary>
    /// Editor window that instantiates BuildingDefinition assets into the scene
    /// and auto-wires a BuildingInteriorController on each one. Replaces the
    /// primitive CreateShop/CreateBank/etc. calls in your old builder script
    /// once you've imported a real asset pack.
    /// </summary>
    public class MaliGoBuildingPlacer : EditorWindow
    {
        [SerializeField] private List<BuildingDefinition> buildings = new List<BuildingDefinition>();

        [MenuItem("Tools/MaliGo/Place Buildings From Definitions")]
        public static void ShowWindow()
        {
            GetWindow<MaliGoBuildingPlacer>("MaliGo Building Placer");
        }

        void OnGUI()
        {
            GUILayout.Label("Drag in BuildingDefinition assets, then click Place.", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedObject so = new SerializedObject(this);
            SerializedProperty prop = so.FindProperty("buildings");
            EditorGUILayout.PropertyField(prop, true);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Place All Buildings In Scene"))
            {
                foreach (var def in buildings)
                {
                    if (def != null) PlaceBuilding(def);
                }
            }
        }

        public static GameObject PlaceBuilding(BuildingDefinition def)
        {
            if (def.exteriorPrefab == null)
            {
                Debug.LogWarning($"BuildingDefinition '{def.displayName}' has no exterior prefab assigned - skipped.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(def.exteriorPrefab);
            instance.name = $"Building_{def.buildingId}";
            instance.transform.position = def.worldPosition;
            instance.transform.eulerAngles = def.eulerRotation;

            if (def.interiorContentPrefab != null)
            {
                GameObject interior = (GameObject)PrefabUtility.InstantiatePrefab(def.interiorContentPrefab, instance.transform);
                interior.name = "InteriorContent";
                interior.transform.localPosition = Vector3.zero;
                interior.transform.localRotation = Quaternion.identity;
            }

            var controller = instance.AddComponent<BuildingInteriorController>();
            controller.autoDetectNames = def.cutawayPartNames;

            var interiorContentTf = instance.transform.Find("InteriorContent");
            if (interiorContentTf != null) controller.interiorOnlyContent = interiorContentTf;

            Undo.RegisterCreatedObjectUndo(instance, "Place MaliGo Building");
            Debug.Log($"✅ Placed {def.displayName} at {def.worldPosition}");
            return instance;
        }
    }
}