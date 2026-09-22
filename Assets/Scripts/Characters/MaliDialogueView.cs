using UnityEngine;
using UnityEngine.UI;

namespace MaliGo.Characters
{
    /// <summary>
    /// Lightweight dialogue panel for Mali. Separate from the gameplay HUD.
    /// </summary>
    public class MaliDialogueView : MonoBehaviour
    {
        static readonly Color DeepForest = new Color(0.059f, 0.369f, 0.180f);
        static readonly Color DarkBrown = new Color(0.33f, 0.22f, 0.12f);

        Canvas canvas;
        GameObject panelRoot;
        Text bodyText;
        Text speakerText;

        void Awake()
        {
            BuildUiIfNeeded();
            HideImmediate();
        }

        void BuildUiIfNeeded()
        {
            if (panelRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("MaliDialogue_Canvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("MaliDialoguePanel");
            panelRoot.transform.SetParent(canvasObject.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 48f);
            panelRect.sizeDelta = new Vector2(760f, 140f);

            var panelImage = panelRoot.AddComponent<Image>();
            Sprite panelSprite = UI.KenneyUiSprites.PanelWarm;
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
            }
            else
            {
                panelImage.color = DeepForest;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            speakerText = CreateText(panelRoot.transform, "Speaker", new Vector2(28f, -18f), new Vector2(704f, 28f), 18, DeepForest, FontStyle.Bold, font);
            speakerText.text = "Mali";

            bodyText = CreateText(panelRoot.transform, "Body", new Vector2(28f, -50f), new Vector2(704f, 68f), 22, DarkBrown, FontStyle.Normal, font);
        }

        static Text CreateText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style, Font font)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var text = obj.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public void Show(string line)
        {
            BuildUiIfNeeded();
            bodyText.text = line;
            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void HideImmediate()
        {
            Hide();
        }
    }
}
