using UnityEngine;

namespace MaliGo.Environment
{
    /// <summary>
    /// Data-driven definition for one building, pointing at prefabs from an
    /// imported low-poly asset pack (Kenney, Synty, etc.) instead of generating
    /// primitives at runtime. Create one asset per building via
    /// Assets > Create > MaliGo > Building Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuildingDefinition", menuName = "MaliGo/Building Definition")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string buildingId;
        public string displayName;

        [Header("Prefabs")]
        [Tooltip("The building shell from your imported asset pack.")]
        public GameObject exteriorPrefab;
        [Tooltip("Optional separate interior prefab (furniture set) instantiated as a child, positioned inside the footprint. Leave empty if the exterior prefab already contains interior detail (e.g. Synty Town Pack houses).")]
        public GameObject interiorContentPrefab;

        [Header("Placement")]
        public Vector3 worldPosition;
        public Vector3 eulerRotation;

        [Header("Cutaway")]
        [Tooltip("Child object names inside the prefab hierarchy that should fade away when the player enters (roof, front wall). Open the prefab and copy the exact names.")]
        public string[] cutawayPartNames = { "Roof" };
    }
}