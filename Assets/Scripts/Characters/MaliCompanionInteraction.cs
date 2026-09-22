using UnityEngine;

namespace MaliGo.Characters
{
    /// <summary>
    /// Handles the temporary "press E to talk to Mali" interaction.
    /// </summary>
    public class MaliCompanionInteraction : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] float interactionRadius = 0.74f;
        [SerializeField] KeyCode interactKey = KeyCode.E;

        [Header("References")]
        [SerializeField] MaliDialogueController dialogueController;
        [SerializeField] MaliNpcController maliController;

        Transform playerTransform;
        GameObject promptRoot;
        UnityEngine.UI.Text promptText;

        public float InteractionRadius => interactionRadius;

        void Awake()
        {
            if (dialogueController == null)
            {
                dialogueController = GetComponent<MaliDialogueController>();
            }

            if (maliController == null)
            {
                maliController = GetComponent<MaliNpcController>();
            }

            if (dialogueController == null)
            {
                dialogueController = gameObject.AddComponent<MaliDialogueController>();
            }

            BuildPromptUi();
            dialogueController.OnDialogueHidden += HandleDialogueHidden;
        }

        void OnDestroy()
        {
            if (dialogueController != null)
            {
                dialogueController.OnDialogueHidden -= HandleDialogueHidden;
            }
        }

        void Update()
        {
            RefreshPlayerReference();
            UpdatePrompt();
            HandleInteractionInput();
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

        void UpdatePrompt()
        {
            if (promptRoot == null)
            {
                return;
            }

            bool showPrompt = IsPlayerInRange() && dialogueController != null && !dialogueController.IsShowingDialogue;
            promptRoot.SetActive(showPrompt);
        }

        void HandleInteractionInput()
        {
            if (dialogueController == null)
            {
                return;
            }

            // Establish eligibility BEFORE asking, because WasInteractPressed() consumes the
            // shared mobile interact request. Polling it while the player is nowhere near Mali
            // swallowed every ACT press meant for a shop, a building or a scenario trigger.
            bool canAct = dialogueController.IsShowingDialogue || IsPlayerInRange();
            if (!canAct || !WasInteractPressed())
            {
                return;
            }

            if (dialogueController.IsShowingDialogue)
            {
                dialogueController.Hide();
                return;
            }

            dialogueController.ShowGreeting();
            maliController?.LookTowardPlayer();
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

        void HandleDialogueHidden()
        {
            UpdatePrompt();
        }

        void BuildPromptUi()
        {
            var canvasObject = new GameObject("MaliPrompt_Canvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;

            var scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            promptRoot = new GameObject("MaliPrompt");
            promptRoot.transform.SetParent(canvasObject.transform, false);

            var rect = promptRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 210f);
            rect.sizeDelta = new Vector2(420f, 36f);

            var image = promptRoot.AddComponent<UnityEngine.UI.Image>();
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

            promptText = textObject.AddComponent<UnityEngine.UI.Text>();
            promptText.font = font;
            promptText.fontSize = 18;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.color = new Color(0.976f, 1f, 0.965f);
            promptText.text = "Press E to talk to Mali";

            promptRoot.SetActive(false);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.875f, 0.643f, 0.392f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
