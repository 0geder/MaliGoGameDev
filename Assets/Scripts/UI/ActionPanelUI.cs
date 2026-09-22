using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MaliGo.UI
{
    /// <summary>
    /// Generic "title + live status text + a row of action buttons" panel, reused by Home
    /// and Bank instead of writing two near-identical bespoke panels. Unlike
    /// ScenarioChoiceUI (which is a one-shot choice that closes immediately), this panel
    /// stays open across multiple actions - each button refreshes the status text in place
    /// so repeated actions (deposit, then contribute to a goal, then deposit again) don't
    /// require re-opening the panel.
    /// </summary>
    public class ActionPanelUI : MonoBehaviour
    {
        static readonly Color DeepForest = new Color(0.059f, 0.369f, 0.180f);
        static readonly Color DarkBrown = new Color(0.33f, 0.22f, 0.12f);

        public struct ActionButton
        {
            public string Label;
            public Action OnClick;

            public ActionButton(string label, Action onClick)
            {
                Label = label;
                OnClick = onClick;
            }
        }

        GameObject panelRoot;
        Text titleText;
        Text bodyText;
        Transform buttonListRoot;
        Func<string> bodyProvider;

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

            var canvasObject = new GameObject("ActionPanel_Canvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("ActionPanel");
            panelRoot.transform.SetParent(canvasObject.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(720f, 460f);

            var panelImage = panelRoot.AddComponent<Image>();
            Sprite panelSprite = KenneyUiSprites.PanelWarm;
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
            }
            else
            {
                panelImage.color = DeepForest;
            }

            titleText = CreateText(panelRoot.transform, "Title", new Vector2(24f, -20f), new Vector2(672f, 36f), 26, DeepForest, FontStyle.Bold);
            bodyText = CreateText(panelRoot.transform, "Body", new Vector2(24f, -64f), new Vector2(672f, 160f), 19, DarkBrown, FontStyle.Normal);

            var listRootObject = new GameObject("Actions");
            listRootObject.transform.SetParent(panelRoot.transform, false);
            var listRect = listRootObject.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0f);
            listRect.anchorMax = new Vector2(1f, 0f);
            listRect.pivot = new Vector2(0.5f, 0f);
            listRect.anchoredPosition = new Vector2(0f, 20f);
            listRect.sizeDelta = new Vector2(-48f, 210f);

            var layout = listRootObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(330f, 60f);
            layout.spacing = new Vector2(12f, 10f);
            layout.childAlignment = TextAnchor.UpperCenter;

            buttonListRoot = listRootObject.transform;

            var closeButton = CreateCloseButton(panelRoot.transform);
            closeButton.onClick.AddListener(Hide);
        }

        Button CreateCloseButton(Transform parent)
        {
            var buttonObject = new GameObject("CloseButton");
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-16f, -16f);
            rect.sizeDelta = new Vector2(44f, 36f);

            var image = buttonObject.AddComponent<Image>();
            Sprite buttonSprite = KenneyUiSprites.ButtonWarm;
            if (buttonSprite != null)
            {
                image.sprite = buttonSprite;
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.color = DarkBrown;
            }

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var labelText = CreateText(buttonObject.transform, "X", Vector2.zero, Vector2.zero, 18, DeepForest, FontStyle.Bold);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = "X";

            return button;
        }

        static Text CreateText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style)
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public void Show(string title, Func<string> bodyTextProvider, IReadOnlyList<ActionButton> actions)
        {
            BuildUiIfNeeded();

            titleText.text = title;
            bodyProvider = bodyTextProvider;
            RefreshBody();

            for (int i = buttonListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(buttonListRoot.GetChild(i).gameObject);
            }

            foreach (var action in actions)
            {
                CreateActionButton(action);
            }

            panelRoot.SetActive(true);
        }

        void RefreshBody()
        {
            if (bodyProvider != null)
            {
                bodyText.text = bodyProvider();
            }
        }

        void CreateActionButton(ActionButton action)
        {
            var buttonObject = new GameObject($"Action_{action.Label}");
            buttonObject.transform.SetParent(buttonListRoot, false);

            var image = buttonObject.AddComponent<Image>();
            Sprite buttonSprite = KenneyUiSprites.ButtonWarm;
            if (buttonSprite != null)
            {
                image.sprite = buttonSprite;
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.color = DarkBrown;
            }

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() =>
            {
                action.OnClick?.Invoke();
                RefreshBody();
            });

            var labelText = CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.zero, 18, DeepForest, FontStyle.Bold);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = action.Label;
        }

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

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
