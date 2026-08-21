using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaliGo.Environment
{
    /// <summary>
    /// Attach to the root of every building (procedural or prefab-based).
    /// Detects when the player enters the building's footprint and fades the
    /// roof + front-facing walls so the interior (modeled inside the same
    /// footprint, in the same scene) is revealed. No load, no scene swap,
    /// no camera cut — the player just keeps walking.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingInteriorController : MonoBehaviour
    {
        [Header("Cutaway Parts")]
        [Tooltip("Renderers that hide when the player is inside (roof, front walls). Auto-populated from child names if left empty.")]
        public List<Renderer> cutawayRenderers = new List<Renderer>();

        [Tooltip("Child object names matched automatically if cutawayRenderers is empty. Match these to your building's hierarchy (procedural names like \"Roof\", or asset-pack names like \"SM_Bld_Roof_01\").")]
        public string[] autoDetectNames = { "Roof", "Roof_Pediment", "Roof_Pyramid", "Awning", "Wall_Front" };

        [Header("Interior")]
        [Tooltip("Optional container of interior-only props (furniture, rugs, counters) shown only while the player is inside.")]
        public Transform interiorOnlyContent;

        [Header("Trigger Zone")]
        [Tooltip("Leave null to auto-generate a BoxCollider trigger sized to this building's combined renderer bounds.")]
        public BoxCollider interiorZone;

        [Header("Fade Settings")]
        public float fadeDuration = 0.25f;
        [Range(0f, 1f)] public float hiddenAlpha = 0.12f; // faint ghost silhouette, not fully invisible

        private readonly List<Material> _fadeMaterials = new List<Material>();
        private Coroutine _fadeRoutine;
        private int _occupantCount; // guards against double triggers from multiple colliders

        void Awake()
        {
            Rigidbody body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
        }

        void Start()
        {
            if (cutawayRenderers.Count == 0)
            {
                AutoDetectCutawayRenderers();
            }

            foreach (var r in cutawayRenderers)
            {
                if (r == null) continue;
                foreach (var mat in r.materials) // .materials instances them - safe to modify per-building
                {
                    MaterialTransparencyUtility.EnableFadeTransparency(mat);
                    _fadeMaterials.Add(mat);
                }
            }

            if (interiorZone == null)
            {
                Transform zoneTransform = transform.Find("InteriorZone");
                if (zoneTransform != null) interiorZone = zoneTransform.GetComponent<BoxCollider>();
                if (interiorZone == null) interiorZone = BuildAutoTrigger();
            }
            interiorZone.isTrigger = true;

            SetInteriorOnlyVisible(false);
        }

        void AutoDetectCutawayRenderers()
        {
            if (autoDetectNames == null) return;

            Transform[] children = GetComponentsInChildren<Transform>(true);
            foreach (var childName in autoDetectNames)
            {
                if (string.IsNullOrWhiteSpace(childName)) continue;

                foreach (Transform child in children)
                {
                    if (child.name != childName || !child.TryGetComponent(out Renderer rend)) continue;
                    if (!cutawayRenderers.Contains(rend)) cutawayRenderers.Add(rend);
                }
            }
        }

        BoxCollider BuildAutoTrigger()
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            if (allRenderers.Length == 0)
            {
                Debug.LogWarning($"{name}: no renderers found to size an interior trigger from.");
                return gameObject.AddComponent<BoxCollider>();
            }

            Bounds bounds = allRenderers[0].bounds;
            foreach (var r in allRenderers) bounds.Encapsulate(r.bounds);

            GameObject zoneObj = new GameObject("InteriorZone_Auto");
            zoneObj.transform.SetParent(transform);
            zoneObj.transform.position = bounds.center;

            BoxCollider box = zoneObj.AddComponent<BoxCollider>();
            box.size = bounds.size; // world-scale bounds, zoneObj has no local scale/rotation offset
            box.isTrigger = true;
            zoneObj.layer = gameObject.layer;
            return box;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _occupantCount++;
            if (_occupantCount == 1) SetHidden(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _occupantCount = Mathf.Max(0, _occupantCount - 1);
            if (_occupantCount == 0) SetHidden(false);
        }

        void SetHidden(bool hidden)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeCutaway(hidden));
            SetInteriorOnlyVisible(hidden);
        }

        void SetInteriorOnlyVisible(bool visible)
        {
            if (interiorOnlyContent != null) interiorOnlyContent.gameObject.SetActive(visible);
        }

        IEnumerator FadeCutaway(bool hidden)
        {
            float targetAlpha = hidden ? hiddenAlpha : 1f;
            float startAlpha = hidden ? 1f : hiddenAlpha;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
                foreach (var mat in _fadeMaterials) MaterialTransparencyUtility.SetAlpha(mat, alpha);
                yield return null;
            }

            foreach (var mat in _fadeMaterials) MaterialTransparencyUtility.SetAlpha(mat, targetAlpha);
        }
    }
}