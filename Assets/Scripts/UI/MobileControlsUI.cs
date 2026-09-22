using MaliGo.Characters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MaliGo.UI
{
    /// <summary>
    /// On-screen virtual joystick (bottom-left) and interact button (bottom-right) for
    /// touch devices. Feeds the existing MaliGoPlayerController.SetVirtualJoystickInput
    /// entrypoint (already present, just never had a UI calling it) and MobileInputBridge
    /// for interaction, so no player-movement or interaction logic is duplicated here.
    /// No touch-control art exists in the project yet, so the visuals are generated
    /// procedurally (two circles) rather than blocked on new assets.
    /// </summary>
    public class MobileControlsUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        const float BaseDiameter = 170f;
        const float HandleDiameter = 78f;

        static readonly Color BaseColor = new Color(0.059f, 0.369f, 0.180f, 0.45f);
        static readonly Color HandleColor = new Color(0.976f, 1f, 0.965f, 0.75f);
        static readonly Color ButtonColor = new Color(0.875f, 0.643f, 0.392f, 0.85f);

        RectTransform joystickBase;
        RectTransform joystickHandle;
        int activeJoystickPointerId = int.MinValue;

        MaliGoPlayerController cachedPlayerController;

        void Awake()
        {
            BuildUi();
        }

        void Update()
        {
            if (cachedPlayerController == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                cachedPlayerController = player != null ? player.GetComponent<MaliGoPlayerController>() : null;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activeJoystickPointerId != int.MinValue)
            {
                return;
            }

            activeJoystickPointerId = eventData.pointerId;
            UpdateHandle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activeJoystickPointerId)
            {
                return;
            }

            UpdateHandle(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activeJoystickPointerId)
            {
                return;
            }

            activeJoystickPointerId = int.MinValue;
            joystickHandle.anchoredPosition = Vector2.zero;
            cachedPlayerController?.SetVirtualJoystickInput(Vector2.zero);
        }

        void UpdateHandle(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBase, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            float maxRadius = (BaseDiameter - HandleDiameter) * 0.5f;
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
            joystickHandle.anchoredPosition = clamped;

            Vector2 normalized = maxRadius > 0.01f ? clamped / maxRadius : Vector2.zero;
            cachedPlayerController?.SetVirtualJoystickInput(normalized);
        }

        void BuildUi()
        {
            var canvasObject = new GameObject("MobileControls_Canvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Sprite circleSprite = CreateCircleSprite(128);

            joystickBase = CreateCircle(canvasObject.transform, "JoystickBase", circleSprite, BaseColor, BaseDiameter,
                new Vector2(0f, 0f), new Vector2(140f, 130f));
            joystickBase.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = true;

            var raycastTarget = joystickBase.GetComponent<Image>();
            raycastTarget.raycastTarget = true;

            var dragHandlerHost = joystickBase.gameObject;
            var forwarder = dragHandlerHost.AddComponent<JoystickDragForwarder>();
            forwarder.owner = this;

            joystickHandle = CreateCircle(joystickBase, "JoystickHandle", circleSprite, HandleColor, HandleDiameter,
                new Vector2(0.5f, 0.5f), Vector2.zero);
            joystickHandle.GetComponent<Image>().raycastTarget = false;

            BuildInteractButton(canvasObject.transform, circleSprite);
        }

        void BuildInteractButton(Transform parent, Sprite circleSprite)
        {
            const float diameter = 130f;

            var buttonRect = CreateCircle(parent, "InteractButton", circleSprite, ButtonColor, diameter,
                new Vector2(1f, 0f), new Vector2(-140f, 130f));

            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonRect.GetComponent<Image>();
            button.onClick.AddListener(MobileInputBridge.RequestInteract);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(buttonRect, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.059f, 0.20f, 0.12f);
            label.text = "ACT";
            label.raycastTarget = false;
        }

        static RectTransform CreateCircle(Transform parent, string name, Sprite sprite, Color color, float diameter, Vector2 anchor, Vector2 anchoredPosition)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(diameter, diameter);
            rect.anchoredPosition = anchoredPosition;

            var image = obj.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Simple;

            return rect;
        }

        static Sprite CreateCircleSprite(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// The joystick base is its own GameObject built by BuildUi() before this component
        /// can be attached with a live 'owner' reference, so drag events are forwarded here
        /// rather than implementing the handlers directly on the base's own object.
        /// </summary>
        class JoystickDragForwarder : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public MobileControlsUI owner;

            public void OnPointerDown(PointerEventData eventData) => owner.OnPointerDown(eventData);
            public void OnDrag(PointerEventData eventData) => owner.OnDrag(eventData);
            public void OnPointerUp(PointerEventData eventData) => owner.OnPointerUp(eventData);
        }
    }
}
