using System;
using UnityEngine;
using UnityEngine.UI;

namespace MaliGo.Scenarios
{
    /// <summary>
    /// Runtime-built panel presenting a ScenarioDefinition's choices as buttons.
    /// Visual style mirrors MaliDialogueView so scenarios feel native to the world.
    /// </summary>
    public class ScenarioChoiceUI : MonoBehaviour
    {
        static readonly Color DeepForest = new Color(0.059f, 0.369f, 0.180f);
        static readonly Color DarkBrown = new Color(0.33f, 0.22f, 0.12f);

        GameObject panelRoot;
        Text titleText;
        Text descriptionText;
        Transform choiceListRoot;

        Action<ScenarioChoice> onChoiceSelected;

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

            var canvasObject = new GameObject("ScenarioChoice_Canvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            panelRoot = new GameObject("ScenarioChoicePanel");
            panelRoot.transform.SetParent(canvasObject.transform, false);

            var panelRect = panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 420f);

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

            titleText = CreateText(panelRoot.transform, "Title", new Vector2(24f, -20f), new Vector2(712f, 40f), 26, DeepForest, FontStyle.Bold);
            descriptionText = CreateText(panelRoot.transform, "Description", new Vector2(24f, -66f), new Vector2(712f, 60f), 20, DarkBrown, FontStyle.Normal);

            var listRootObject = new GameObject("Choices");
            listRootObject.transform.SetParent(panelRoot.transform, false);
            var listRect = listRootObject.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0f);
            listRect.anchorMax = new Vector2(1f, 0f);
            listRect.pivot = new Vector2(0.5f, 0f);
            listRect.anchoredPosition = new Vector2(0f, 24f);
            listRect.sizeDelta = new Vector2(-48f, 300f);

            var layout = listRootObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            choiceListRoot = listRootObject.transform;
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

        public void Show(ScenarioDefinition scenario, Action<ScenarioChoice> onSelected)
        {
            BuildUiIfNeeded();

            onChoiceSelected = onSelected;
            titleText.text = scenario.title;
            descriptionText.text = scenario.description;

            for (int i = choiceListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(choiceListRoot.GetChild(i).gameObject);
            }

            foreach (var choice in scenario.choices)
            {
                CreateChoiceButton(choice);
            }

            panelRoot.SetActive(true);
        }

        void CreateChoiceButton(ScenarioChoice choice)
        {
            var buttonObject = new GameObject($"Choice_{choice.choiceId}");
            buttonObject.transform.SetParent(choiceListRoot, false);

            var layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 76f;

            var image = buttonObject.AddComponent<Image>();
            Sprite buttonSprite = UI.KenneyUiSprites.ButtonWarm;
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
            button.onClick.AddListener(() => HandleChoiceClicked(choice));

            var labelText = CreateText(buttonObject.transform, "Label", new Vector2(16f, -8f), new Vector2(680f, 28f), 20, DeepForest, FontStyle.Bold);
            labelText.text = choice.label;

            var descText = CreateText(buttonObject.transform, "Consequence", new Vector2(16f, -38f), new Vector2(680f, 28f), 15, DarkBrown, FontStyle.Normal);
            descText.text = choice.description;
        }

        void HandleChoiceClicked(ScenarioChoice choice)
        {
            Hide();
            onChoiceSelected?.Invoke(choice);
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
