using UnityEngine;
using UnityEngine.UI;

namespace MaliGo.World
{
    /// <summary>
    /// Shared "walk up, press E" proximity interaction for world locations that aren't a
    /// ScenarioDefinition trade-off decision (Home, Bank, Work). Mirrors ScenarioTrigger's
    /// interaction pattern (same prompt UI, same input handling, mobile-bridge included)
    /// without coupling to the scenario system.
    /// </summary>
    public abstract class ProximityInteraction : MonoBehaviour
    {
        [SerializeField] float interactionRadius = 0.7f;
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] string promptText = "Press E";

        Transform playerTransform;
        GameObject promptRoot;
        Text promptLabel;

        protected abstract void OnInteract();

        /// <summary>Override to block interaction while e.g. this location's own panel is open.</summary>
        protected virtual bool CanInteract() => true;

        protected void SetPrompt(string text)
        {
            promptText = text;
            if (promptLabel != null)
            {
                promptLabel.text = promptText;
            }
        }

        void Awake()
        {
            BuildPromptUi();
            OnAwake();
        }

        /// <summary>
        /// Override instead of Awake() - a subclass's own Awake() would hide this base
        /// Awake() (Unity calls only the most-derived one), skipping BuildPromptUi().
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        void Update()
        {
            RefreshPlayerReference();
            bool inRange = IsPlayerInRange();
            bool canInteract = inRange && CanInteract();

            if (promptRoot != null)
            {
                promptRoot.SetActive(canInteract);
            }

            if (canInteract && WasInteractPressed())
            {
                OnInteract();
            }
        }

        void RefreshPlayerReference()
        {
            if (playerTransform != null)
            {
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            playerTransform = player != null ? player.transform : null;
        }

        bool IsPlayerInRange()
        {
            if (playerTransform == null)
            {
                return false;
            }

            Vector3 flatDelta = playerTransform.position - transform.position;
            flatDelta.y = 0f;
            return flatDelta.sqrMagnitude <= interactionRadius * interactionRadius;
        }

        bool WasInteractPressed()
        {
            if (UI.MobileInputBridge.ConsumeInteractRequest())
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(interactKey);
#else
            return false;
#endif
        }

        void BuildPromptUi()
        {
            var canvasObject = new GameObject($"{gameObject.name}_PromptCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            promptRoot = new GameObject("Prompt");
            promptRoot.transform.SetParent(canvasObject.transform, false);

            var rect = promptRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 210f);
            rect.sizeDelta = new Vector2(420f, 36f);

            var image = promptRoot.AddComponent<Image>();
            image.color = new Color(0.059f, 0.369f, 0.180f, 0.88f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var textObject = new GameObject("PromptText");
            textObject.transform.SetParent(promptRoot.transform, false);
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 4f);
            textRect.offsetMax = new Vector2(-12f, -4f);

            promptLabel = textObject.AddComponent<Text>();
            promptLabel.font = font;
            promptLabel.fontSize = 18;
            promptLabel.alignment = TextAnchor.MiddleCenter;
            promptLabel.color = new Color(0.976f, 1f, 0.965f);
            promptLabel.text = promptText;

            promptRoot.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.259f, 0.520f, 0.780f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
