using System.Collections.Generic;
using MaliGo.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Multi-step character creation flow: name, appearance, financial mirror, confirmation.
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        static readonly Color MossGreen = new Color(0.031f, 0.478f, 0.094f);
        static readonly Color Sage = new Color(0.788f, 0.937f, 0.706f);
        static readonly Color Cream = new Color(0.976f, 1.000f, 0.965f);
        static readonly Color GoldenAmber = new Color(0.875f, 0.643f, 0.392f);
        static readonly Color DeepPanel = new Color(0.059f, 0.369f, 0.180f, 0.95f);

        readonly Dictionary<string, string> appearanceSelections = new Dictionary<string, string>();
        readonly FinancialMirrorAnswers mirrorAnswers = new FinancialMirrorAnswers();

        Canvas canvas;
        Font uiFont;
        GameObject stepRoot;
        InputField nameInput;
        Text stepIndicatorText;
        Text titleText;
        Text bodyText;
        Text errorText;
        Button nextButton;

        PlayerData draftPlayer;
        int currentStep;

        void Awake()
        {
            GameFlowController.EnsurePlayerDataManager();
            draftPlayer = PlayerData.CreateNew();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildCanvas();
            ShowStep(0);
        }

        void BuildCanvas()
        {
            var canvasObj = new GameObject("CharacterCreation_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            var bg = CreatePanel(canvasObj.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MossGreen);

            var panel = CreatePanel(bg.transform, "MainPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420f, -280f), new Vector2(420f, 280f), DeepPanel);

            stepIndicatorText = CreateText(panel.transform, "StepIndicator", new Vector2(24f, -20f), new Vector2(772f, 28f), "Step 1 of 4", 16, Sage, FontStyle.Normal);
            titleText = CreateText(panel.transform, "Title", new Vector2(24f, -52f), new Vector2(772f, 44f), "", 30, Cream, FontStyle.Bold);
            bodyText = CreateText(panel.transform, "Body", new Vector2(24f, -104f), new Vector2(772f, 60f), "", 18, Cream, FontStyle.Normal);
            errorText = CreateText(panel.transform, "Error", new Vector2(24f, -420f), new Vector2(772f, 28f), "", 16, GoldenAmber, FontStyle.Italic);

            stepRoot = new GameObject("StepContent");
            stepRoot.transform.SetParent(panel.transform, false);
            var stepRect = stepRoot.AddComponent<RectTransform>();
            stepRect.anchorMin = new Vector2(0f, 0f);
            stepRect.anchorMax = new Vector2(1f, 1f);
            stepRect.offsetMin = new Vector2(24f, 96f);
            stepRect.offsetMax = new Vector2(-24f, -170f);

            CreateButton(panel.transform, "BackButton", new Vector2(24f, 24f), new Vector2(140f, 48f), "Back", Sage, OnBackClicked);
            nextButton = CreateButton(panel.transform, "NextButton", new Vector2(656f, 24f), new Vector2(140f, 48f), "Next", GoldenAmber, OnNextClicked);

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        void ShowStep(int step)
        {
            currentStep = step;
            ClearStepRoot();
            errorText.text = string.Empty;
            stepIndicatorText.text = $"Step {step + 1} of 4";

            switch (step)
            {
                case 0:
                    ShowNameStep();
                    break;
                case 1:
                    ShowAppearanceStep();
                    break;
                case 2:
                    ShowFinancialMirrorStep();
                    break;
                case 3:
                    ShowCompletionStep();
                    break;
            }

            if (nextButton != null)
            {
                var label = nextButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = step >= 3 ? "Begin Journey" : "Next";
                }
            }
        }

        void ShowNameStep()
        {
            titleText.text = "What should we call you?";
            bodyText.text = "Enter the name you'd like to use on your MaliGo journey.";

            var inputRoot = CreatePanel(stepRoot.transform, "NameInputRoot", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -80f), new Vector2(0f, -20f), new Color(0f, 0f, 0f, 0.25f));
            nameInput = CreateInputField(inputRoot.transform, draftPlayer.characterName, "Your name");
        }

        void ShowAppearanceStep()
        {
            titleText.text = "Create your character";
            bodyText.text = "Choose how you'd like to appear. Visual assets will expand in a future update.";

            var options = new (string key, string label, string[] choices)[]
            {
                ("skinTone", "Skin tone", new[] { "light", "medium", "deep" }),
                ("hairstyle", "Hairstyle", new[] { "short", "medium", "long", "bald" }),
                ("hairColor", "Hair colour", new[] { "black", "brown", "blonde", "red" }),
                ("clothing", "Clothing", new[] { "casual", "smart", "sporty" }),
                ("accessories", "Accessories", new[] { "none", "glasses", "hat", "backpack" }),
                ("genderPresentation", "Presentation", new[] { "feminine", "masculine", "neutral" }),
                ("bodyType", "Body type", new[] { "slim", "average", "broad" })
            };

            float y = 0f;
            foreach (var option in options)
            {
                if (!appearanceSelections.ContainsKey(option.key))
                {
                    appearanceSelections[option.key] = option.choices[0];
                }

                CreateText(stepRoot.transform, $"Label_{option.key}", new Vector2(0f, y), new Vector2(220f, 28f), option.label, 16, Sage, FontStyle.Bold);
                CreateDropdown(stepRoot.transform, option.key, new Vector2(230f, y), new Vector2(500f, 32f), option.choices, appearanceSelections[option.key]);
                y -= 44f;
            }
        }

        void ShowFinancialMirrorStep()
        {
            titleText.text = "Let's understand your financial life";
            bodyText.text = "Answer honestly — this helps tailor your experience. Your answers stay private.";

            float y = 0f;
            CreateLifeChapterSelector(ref y);
            CreateDropdownRow("Income type", "incomeType", ref y, new[] { "salary", "part_time", "allowance", "self_employed", "mixed" }, mirrorAnswers.incomeType);
            CreateDropdownRow("Spending style", "spendingBehaviour", ref y, new[] { "careful", "balanced", "impulsive" }, mirrorAnswers.spendingBehaviour);
            CreateDropdownRow("Saving habit", "savingBehaviour", ref y, new[] { "consistent", "sometimes", "rarely" }, mirrorAnswers.savingBehaviour);
            CreateDropdownRow("Primary goal", "primaryGoal", ref y, new[] { "emergency_fund", "debt", "home", "invest", "education" }, mirrorAnswers.primaryGoal);
            CreateDropdownRow("Risk comfort", "riskTolerance", ref y, new[] { "conservative", "moderate", "aggressive" }, mirrorAnswers.riskTolerance);
            CreateDropdownRow("Biggest challenge", "primaryChallenge", ref y, new[] { "budgeting", "debt", "saving", "investing", "income" }, mirrorAnswers.primaryChallenge);
            CreateConfidenceSlider(ref y);
        }

        void ShowCompletionStep()
        {
            titleText.text = "Your journey begins";
            bodyText.text = "Review your profile, then enter MaliGo World.";

            var profile = FinancialMirrorCalculator.Calculate(mirrorAnswers);
            string summary =
                $"Name: {draftPlayer.characterName}\n" +
                $"Life chapter: {FormatLifeChapter(mirrorAnswers.lifeChapter)}\n" +
                $"Primary goal: {FormatToken(profile.primaryGoal)}\n" +
                $"Confidence: {profile.financialConfidence}/5\n\n" +
                "You're ready to explore your neighbourhood and build healthier money habits.";

            CreateText(stepRoot.transform, "Summary", new Vector2(0f, 0f), new Vector2(760f, 280f), summary, 20, Cream, FontStyle.Normal);
        }

        void CreateLifeChapterSelector(ref float y)
        {
            CreateText(stepRoot.transform, "LifeChapterLabel", new Vector2(0f, y), new Vector2(220f, 28f), "Life chapter", 16, Sage, FontStyle.Bold);

            var chapters = new[]
            {
                LifeChapter.STUDENT,
                LifeChapter.YOUNG_PROFESSIONAL,
                LifeChapter.ESTABLISHED_ADULT,
                LifeChapter.WEALTH_BUILDER,
                LifeChapter.FINANCIAL_INDEPENDENCE
            };

            var labels = new List<string>();
            foreach (var chapter in chapters)
            {
                labels.Add(FormatLifeChapter(chapter));
            }

            CreateDropdown(stepRoot.transform, "lifeChapter", new Vector2(230f, y), new Vector2(500f, 32f), labels.ToArray(), FormatLifeChapter(mirrorAnswers.lifeChapter));
            y -= 44f;
        }

        void CreateDropdownRow(string label, string key, ref float y, string[] options, string current)
        {
            CreateText(stepRoot.transform, $"Label_{key}", new Vector2(0f, y), new Vector2(220f, 28f), label, 16, Sage, FontStyle.Bold);
            CreateDropdown(stepRoot.transform, key, new Vector2(230f, y), new Vector2(500f, 32f), options, current);
            y -= 44f;
        }

        void CreateConfidenceSlider(ref float y)
        {
            CreateText(stepRoot.transform, "ConfidenceLabel", new Vector2(0f, y), new Vector2(220f, 28f), "Financial confidence", 16, Sage, FontStyle.Bold);

            var sliderObj = new GameObject("ConfidenceSlider");
            sliderObj.transform.SetParent(stepRoot.transform, false);
            var rect = sliderObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(230f, y);
            rect.sizeDelta = new Vector2(420f, 24f);

            var bg = sliderObj.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.3f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(8f, 6f);
            fillAreaRect.offsetMax = new Vector2(-8f, -6f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = GoldenAmber;

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(8f, 0f);
            handleAreaRect.offsetMax = new Vector2(-8f, 0f);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, 18f);
            handle.AddComponent<Image>().color = Cream;

            var slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 1;
            slider.maxValue = 5;
            slider.wholeNumbers = true;
            slider.value = mirrorAnswers.financialConfidence;
            slider.onValueChanged.AddListener(v => mirrorAnswers.financialConfidence = Mathf.RoundToInt(v));

            var valueText = CreateText(stepRoot.transform, "ConfidenceValue", new Vector2(660f, y), new Vector2(70f, 28f), $"{mirrorAnswers.financialConfidence}/5", 16, Cream, FontStyle.Normal);
            slider.onValueChanged.AddListener(v => valueText.text = $"{Mathf.RoundToInt(v)}/5");
            y -= 44f;
        }

        void OnBackClicked()
        {
            if (currentStep == 0)
            {
                return;
            }

            if (!TryCaptureCurrentStep())
            {
                return;
            }

            ShowStep(currentStep - 1);
        }

        void OnNextClicked()
        {
            if (!TryCaptureCurrentStep())
            {
                return;
            }

            if (currentStep >= 3)
            {
                CompleteCharacterCreation();
                return;
            }

            ShowStep(currentStep + 1);
        }

        bool TryCaptureCurrentStep()
        {
            errorText.text = string.Empty;

            switch (currentStep)
            {
                case 0:
                    string name = nameInput != null ? nameInput.text.Trim() : draftPlayer.characterName;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        errorText.text = "Please enter a name to continue.";
                        return false;
                    }

                    draftPlayer.characterName = name;
                    return true;

                case 1:
                    draftPlayer.appearance = BuildAppearanceData();
                    return true;

                case 2:
                    CaptureMirrorAnswersFromUI();
                    return true;

                default:
                    return true;
            }
        }

        void CaptureMirrorAnswersFromUI()
        {
            foreach (var dropdown in stepRoot.GetComponentsInChildren<Dropdown>(true))
            {
                string key = dropdown.gameObject.name.Replace("Dropdown_", string.Empty);
                string value = dropdown.options[dropdown.value].text;

                switch (key)
                {
                    case "lifeChapter":
                        mirrorAnswers.lifeChapter = ParseLifeChapter(value);
                        break;
                    case "incomeType":
                        mirrorAnswers.incomeType = NormalizeToken(value);
                        break;
                    case "spendingBehaviour":
                        mirrorAnswers.spendingBehaviour = NormalizeToken(value);
                        break;
                    case "savingBehaviour":
                        mirrorAnswers.savingBehaviour = NormalizeToken(value);
                        break;
                    case "primaryGoal":
                        mirrorAnswers.primaryGoal = NormalizeToken(value);
                        break;
                    case "riskTolerance":
                        mirrorAnswers.riskTolerance = NormalizeToken(value);
                        break;
                    case "primaryChallenge":
                        mirrorAnswers.primaryChallenge = NormalizeToken(value);
                        break;
                    default:
                        appearanceSelections[key] = NormalizeToken(value);
                        break;
                }
            }
        }

        AppearanceData BuildAppearanceData()
        {
            return new AppearanceData
            {
                skinTone = GetSelection("skinTone", "medium"),
                hairstyle = GetSelection("hairstyle", "short"),
                hairColor = GetSelection("hairColor", "black"),
                clothing = GetSelection("clothing", "casual"),
                accessories = GetSelection("accessories", "none"),
                genderPresentation = GetSelection("genderPresentation", "neutral"),
                bodyType = GetSelection("bodyType", "average")
            };
        }

        string GetSelection(string key, string fallback)
        {
            return appearanceSelections.TryGetValue(key, out var value) ? value : fallback;
        }

        void CompleteCharacterCreation()
        {
            draftPlayer.currentLifeChapter = FinancialMirrorCalculator.DetermineLifeChapter(mirrorAnswers);
            draftPlayer.financialProfile = FinancialMirrorCalculator.Calculate(mirrorAnswers);
            draftPlayer.financialStats = FinancialStats.CreateDefaults(draftPlayer.currentLifeChapter);
            draftPlayer.goals = new[] { CreateGoalFromProfile(draftPlayer.financialProfile) };
            draftPlayer.isCharacterCreated = true;

            PlayerDataManager.Instance.SetPlayerData(draftPlayer, saveImmediately: true);
            GameFlowController.LoadWorldScene();
        }

        static FinancialGoal CreateGoalFromProfile(FinancialProfile profile)
        {
            switch (profile.primaryGoal)
            {
                case "debt":
                    return new FinancialGoal("goal_debt", "Pay Down Debt", 10000f, 2500f, 150f, "In Progress", "Debt");
                case "home":
                    return new FinancialGoal("goal_home", "Home Deposit", 80000f, 12000f, 300f, "In Progress", "Housing");
                case "invest":
                    return new FinancialGoal("goal_invest", "Start Investing", 5000f, 800f, 100f, "In Progress", "Investing");
                case "education":
                    return new FinancialGoal("goal_education", "Education Fund", 15000f, 2000f, 120f, "In Progress", "Education");
                default:
                    return new FinancialGoal("goal_emergency", "Emergency Fund", 10000f, 1500f, 200f, "In Progress", "Safety Net");
            }
        }

        void ClearStepRoot()
        {
            if (stepRoot == null)
            {
                return;
            }

            for (int i = stepRoot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(stepRoot.transform.GetChild(i).gameObject);
            }
        }

        static string FormatLifeChapter(LifeChapter chapter)
        {
            return chapter switch
            {
                LifeChapter.STUDENT => "Student",
                LifeChapter.YOUNG_PROFESSIONAL => "Young Professional",
                LifeChapter.ESTABLISHED_ADULT => "Established Adult",
                LifeChapter.WEALTH_BUILDER => "Wealth Builder",
                LifeChapter.FINANCIAL_INDEPENDENCE => "Financial Independence",
                _ => chapter.ToString()
            };
        }

        static LifeChapter ParseLifeChapter(string label)
        {
            return label switch
            {
                "Student" => LifeChapter.STUDENT,
                "Young Professional" => LifeChapter.YOUNG_PROFESSIONAL,
                "Established Adult" => LifeChapter.ESTABLISHED_ADULT,
                "Wealth Builder" => LifeChapter.WEALTH_BUILDER,
                "Financial Independence" => LifeChapter.FINANCIAL_INDEPENDENCE,
                _ => LifeChapter.YOUNG_PROFESSIONAL
            };
        }

        static string FormatToken(string token)
        {
            return token.Replace('_', ' ');
        }

        static string NormalizeToken(string label)
        {
            return label.ToLowerInvariant().Replace(' ', '_');
        }

        GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            obj.AddComponent<Image>().color = color;
            return obj;
        }

        Text CreateText(Transform parent, string name, Vector2 pos, Vector2 size, string content, int fontSize, Color color, FontStyle style)
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
            text.text = content;
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            return text;
        }

        Button CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var obj = CreatePanel(parent, name, new Vector2(0f, 0f), new Vector2(0f, 0f), pos, pos + size, color);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var button = obj.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            var text = CreateText(obj.transform, "Label", new Vector2(12f, -8f), new Vector2(size.x - 24f, size.y - 8f), label, 18, Cream, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        InputField CreateInputField(Transform parent, string initialValue, string placeholder)
        {
            var obj = new GameObject("NameInput");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 8f);
            rect.offsetMax = new Vector2(-12f, -8f);
            obj.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);
            var text = textObj.AddComponent<Text>();
            text.font = uiFont;
            text.fontSize = 20;
            text.color = Cream;
            text.supportRichText = false;

            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(obj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10f, 6f);
            placeholderRect.offsetMax = new Vector2(-10f, -6f);
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = placeholder;
            placeholderText.font = uiFont;
            placeholderText.fontSize = 20;
            placeholderText.color = new Color(Cream.r, Cream.g, Cream.b, 0.45f);
            placeholderText.fontStyle = FontStyle.Italic;

            var input = obj.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholderText;
            input.text = initialValue ?? string.Empty;
            return input;
        }

        void CreateDropdown(Transform parent, string key, Vector2 pos, Vector2 size, string[] options, string current)
        {
            var obj = new GameObject($"Dropdown_{key}");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            obj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 2f);
            labelRect.offsetMax = new Vector2(-30f, -2f);
            var label = labelObj.AddComponent<Text>();
            label.font = uiFont;
            label.fontSize = 16;
            label.color = Cream;
            label.alignment = TextAnchor.MiddleLeft;

            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(obj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-10f, 0f);
            arrowRect.sizeDelta = new Vector2(16f, 16f);
            var arrow = arrowObj.AddComponent<Text>();
            arrow.text = "▼";
            arrow.font = uiFont;
            arrow.fontSize = 14;
            arrow.color = Sage;
            arrow.alignment = TextAnchor.MiddleCenter;

            var template = new GameObject("Template");
            template.transform.SetParent(obj.transform, false);
            template.SetActive(false);
            var templateRect = template.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = Vector2.zero;
            templateRect.sizeDelta = new Vector2(0f, 150f);
            template.AddComponent<Image>().color = DeepPanel;

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = DeepPanel;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 28f);

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 28f);
            var itemToggle = item.AddComponent<Toggle>();

            var itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);
            var itemBgRect = itemBg.AddComponent<RectTransform>();
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            itemBg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            var itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(item.transform, false);
            var itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(10f, 1f);
            itemLabelRect.offsetMax = new Vector2(-10f, -1f);
            var itemLabel = itemLabelObj.AddComponent<Text>();
            itemLabel.font = uiFont;
            itemLabel.fontSize = 16;
            itemLabel.color = Cream;
            itemLabel.alignment = TextAnchor.MiddleLeft;

            itemToggle.targetGraphic = itemBg.GetComponent<Image>();
            itemToggle.graphic = null;

            var dropdown = obj.AddComponent<Dropdown>();
            dropdown.targetGraphic = obj.GetComponent<Image>();
            dropdown.captionText = label;
            dropdown.itemText = itemLabel;
            dropdown.template = templateRect;
            dropdown.options.Clear();

            int selectedIndex = 0;
            for (int i = 0; i < options.Length; i++)
            {
                dropdown.options.Add(new Dropdown.OptionData(options[i]));
                if (options[i] == current || NormalizeToken(options[i]) == current)
                {
                    selectedIndex = i;
                }
            }

            dropdown.value = selectedIndex;
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(index =>
            {
                string selected = options[index];
                if (key == "lifeChapter")
                {
                    mirrorAnswers.lifeChapter = ParseLifeChapter(selected);
                }
                else if (appearanceSelections.ContainsKey(key) || currentStep == 1)
                {
                    appearanceSelections[key] = NormalizeToken(selected);
                }
                else
                {
                    switch (key)
                    {
                        case "incomeType": mirrorAnswers.incomeType = NormalizeToken(selected); break;
                        case "spendingBehaviour": mirrorAnswers.spendingBehaviour = NormalizeToken(selected); break;
                        case "savingBehaviour": mirrorAnswers.savingBehaviour = NormalizeToken(selected); break;
                        case "primaryGoal": mirrorAnswers.primaryGoal = NormalizeToken(selected); break;
                        case "riskTolerance": mirrorAnswers.riskTolerance = NormalizeToken(selected); break;
                        case "primaryChallenge": mirrorAnswers.primaryChallenge = NormalizeToken(selected); break;
                    }
                }
            });
        }
    }
}
