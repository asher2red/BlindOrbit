using BlindOrbit.Gameplay;
using BlindOrbit.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BlindOrbit.UI
{
    public sealed class UIManager : MonoBehaviour
    {
        StageManager stageManager;
        LifeManager lifeManager;
        ScoreManager scoreManager;
        FuelSystem fuelSystem;
        PlayerController player;

        Slider fuelSlider;
        Image fuelFillImage;
        Text speedText;
        Text stageText;
        Text markerText;
        Text livesText;
        Text scoreText;
        RectTransform goalArrow;
        GameObject failurePanel;
        GameObject clearPanel;
        Text failureReasonText;
        Text clearStatsText;
        Button nextButton;
        Button pauseButton;
        Text pauseButtonText;
        UnityEngine.Events.UnityAction clearNextAction;
        float displayedFuel = -1f;
        int displayedSpeedTenths = -1;
        int displayedLives = -1;
        int displayedScore = -1;
        float markerFeedbackUntil;
        bool markerFeedbackActive;
        int displayedMarkerCount = -1;
        int displayedMarkerMax = -1;
        readonly Color fuelNormalColor = new Color(0.2f, 0.95f, 0.72f, 1f);
        readonly Color fuelCriticalColor = new Color(1f, 0.18f, 0.16f, 1f);
        readonly Color fuelCriticalDimColor = new Color(0.58f, 0.04f, 0.04f, 1f);
        readonly Color markerNormalColor = new Color(0.84f, 0.96f, 1f, 1f);
        readonly Color markerLimitColor = new Color(1f, 0.16f, 0.14f, 1f);
        readonly Color markerLimitFlashColor = new Color(1f, 0.82f, 0.82f, 1f);

        public void Initialize(StageManager manager, LifeManager lives, ScoreManager score)
        {
            stageManager = manager;
            lifeManager = lives;
            scoreManager = score;
            stageManager.MarkerCountChanged += UpdateMarkerCount;
            lifeManager.LivesChanged += UpdateLives;
            scoreManager.ScoreChanged += UpdateScore;
            EnsureEventSystem();
            BuildCanvas();
            UpdateMarkerCount(stageManager.ActiveMarkerCount, stageManager.MaxMarkerCount);
            UpdateLives(lifeManager.Lives);
            UpdateScore(scoreManager.CurrentScore);
        }

        public void BindPlayer(PlayerController newPlayer, FuelSystem newFuel, string stageName)
        {
            player = newPlayer;
            fuelSystem = newFuel;
            stageText.text = stageName;
            player.SetTurnLeftHeld(false);
            player.SetForwardHeld(false);
            player.SetTurnRightHeld(false);
            displayedFuel = -1f;
            displayedSpeedTenths = -1;
            displayedLives = -1;
            displayedScore = -1;
            HidePanels();
            SetPauseVisual(false);
            UpdateLives(lifeManager.Lives);
            UpdateScore(scoreManager.CurrentScore);
        }

        public void ShowFailure(string reason)
        {
            failurePanel.SetActive(true);
            failureReasonText.text = reason;
        }

        public void ShowClear(StageScoreBreakdown breakdown, bool hasNext, UnityEngine.Events.UnityAction nextOverride)
        {
            clearPanel.SetActive(true);
            clearStatsText.text = FormatScoreBreakdown(breakdown);
            nextButton.interactable = hasNext;
            clearNextAction = nextOverride;
            if (nextOverride != null)
            {
                nextButton.interactable = true;
            }
        }

        public void HidePanels()
        {
            failurePanel.SetActive(false);
            clearPanel.SetActive(false);
            clearNextAction = null;
        }

        public void SetPauseVisual(bool paused)
        {
            pauseButtonText.text = paused ? "Resume" : "Pause";
        }

        public void UpdateMarkerCount(int count, int maxCount)
        {
            if (markerText == null || count == displayedMarkerCount && maxCount == displayedMarkerMax)
            {
                return;
            }

            displayedMarkerCount = count;
            displayedMarkerMax = maxCount;
            markerText.text = $"Markers: {count} / {maxCount}";
            if (!markerFeedbackActive)
            {
                markerText.color = MarkerTextBaseColor();
            }
        }

        public void ShowMarkerLimitFeedback()
        {
            markerFeedbackUntil = Time.unscaledTime + 0.45f;
            markerFeedbackActive = true;
        }

        void OnDestroy()
        {
            if (stageManager != null)
            {
                stageManager.MarkerCountChanged -= UpdateMarkerCount;
            }

            if (lifeManager != null)
            {
                lifeManager.LivesChanged -= UpdateLives;
            }

            if (scoreManager != null)
            {
                scoreManager.ScoreChanged -= UpdateScore;
            }
        }

        void Update()
        {
            if (fuelSystem != null)
            {
                UpdateFuelGauge(fuelSystem.NormalizedFuel);
            }

            if (player != null)
            {
                UpdateSpeedText(player.Velocity.magnitude);
                var toGoal = stageManager.GoalPosition - (Vector2)player.transform.position;
                var angle = Mathf.Atan2(toGoal.y, toGoal.x) * Mathf.Rad2Deg - 90f;
                goalArrow.localRotation = Quaternion.Euler(0f, 0f, angle);
            }

            UpdateMarkerFeedback();
        }

        void BuildCanvas()
        {
            var canvasObject = new GameObject("Blind Orbit UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;

            var root = canvasObject.GetComponent<RectTransform>();
            BuildGameplayHud(root);
            BuildFailurePanel(root);
            BuildClearPanel(root);
        }

        void BuildGameplayHud(RectTransform root)
        {
            var topBar = CreatePanel("Gameplay HUD", root, new Color(0f, 0f, 0f, 0.35f));
            Anchor(topBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -156f), new Vector2(0f, 0f));

            stageText = CreateText("Stage", topBar, "01 - First Orbit", 34, TextAnchor.MiddleLeft);
            Anchor(stageText.rectTransform, new Vector2(0.04f, 0.52f), new Vector2(0.54f, 0.92f), Vector2.zero, Vector2.zero);

            fuelSlider = CreateSlider("Fuel Gauge", topBar);
            fuelFillImage = fuelSlider.fillRect.GetComponent<Image>();
            Anchor(fuelSlider.GetComponent<RectTransform>(), new Vector2(0.04f, 0.2f), new Vector2(0.52f, 0.42f), Vector2.zero, Vector2.zero);

            speedText = CreateText("Speed", topBar, "0.0 m/s", 30, TextAnchor.MiddleRight);
            Anchor(speedText.rectTransform, new Vector2(0.54f, 0.18f), new Vector2(0.74f, 0.52f), Vector2.zero, Vector2.zero);

            livesText = CreateText("Lives", topBar, "Lives: 5", 30, TextAnchor.MiddleRight);
            Anchor(livesText.rectTransform, new Vector2(0.54f, 0.56f), new Vector2(0.74f, 0.9f), Vector2.zero, Vector2.zero);

            markerText = CreateText("Marker Count", topBar, "Markers: 0 / 5", 28, TextAnchor.MiddleLeft);
            Anchor(markerText.rectTransform, new Vector2(0.04f, -0.12f), new Vector2(0.52f, 0.08f), Vector2.zero, Vector2.zero);

            scoreText = CreateText("Score", topBar, "Score: 0", 28, TextAnchor.MiddleLeft);
            Anchor(scoreText.rectTransform, new Vector2(0.04f, -0.34f), new Vector2(0.52f, -0.14f), Vector2.zero, Vector2.zero);

            goalArrow = CreateText("Goal Arrow", topBar, "^", 64, TextAnchor.MiddleCenter).rectTransform;
            Anchor(goalArrow, new Vector2(0.86f, 0.16f), new Vector2(0.94f, 0.62f), Vector2.zero, Vector2.zero);

            pauseButton = CreateButton("Pause Button", topBar, "Pause", stageManager.TogglePause);
            pauseButtonText = pauseButton.GetComponentInChildren<Text>();
            Anchor(pauseButton.GetComponent<RectTransform>(), new Vector2(0.76f, 0.58f), new Vector2(0.96f, 0.9f), Vector2.zero, Vector2.zero);

            BuildControlButtons(root);
        }

        void BuildControlButtons(RectTransform root)
        {
            var controls = CreatePanel("Flight Controls", root, new Color(0f, 0f, 0f, 0.24f));
            Anchor(controls, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 210f));

            var leftButton = CreateHoldButton("Turn Left Button", controls, "LEFT", isHeld => player?.SetTurnLeftHeld(isHeld));
            Anchor(leftButton.GetComponent<RectTransform>(), new Vector2(0.04f, 0.18f), new Vector2(0.25f, 0.82f), Vector2.zero, Vector2.zero);

            var forwardButton = CreateHoldButton("Forward Button", controls, "FORWARD", isHeld => player?.SetForwardHeld(isHeld));
            Anchor(forwardButton.GetComponent<RectTransform>(), new Vector2(0.28f, 0.12f), new Vector2(0.52f, 0.88f), Vector2.zero, Vector2.zero);

            var rightButton = CreateHoldButton("Turn Right Button", controls, "RIGHT", isHeld => player?.SetTurnRightHeld(isHeld));
            Anchor(rightButton.GetComponent<RectTransform>(), new Vector2(0.55f, 0.18f), new Vector2(0.76f, 0.82f), Vector2.zero, Vector2.zero);

            var markerButton = CreateButton("Place Marker Button", controls, "MARK", stageManager.PlaceMarker);
            markerButton.GetComponentInChildren<Text>().fontSize = 28;
            Anchor(markerButton.GetComponent<RectTransform>(), new Vector2(0.79f, 0.18f), new Vector2(0.96f, 0.82f), Vector2.zero, Vector2.zero);
        }

        void BuildFailurePanel(RectTransform root)
        {
            failurePanel = CreateOverlayPanel("Failure Panel", root);
            CreateText("Title", failurePanel.transform, "Mission Failed", 54, TextAnchor.MiddleCenter);
            Anchor((RectTransform)failurePanel.transform.Find("Title"), new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.68f), Vector2.zero, Vector2.zero);

            CreateText("Reason", failurePanel.transform, "", 28, TextAnchor.MiddleCenter);
            failureReasonText = failurePanel.transform.Find("Reason").GetComponent<Text>();
            Anchor(failureReasonText.rectTransform, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.55f), Vector2.zero, Vector2.zero);

            var retry = CreateButton("Retry Button", failurePanel.transform, "Retry", stageManager.RetryStage);
            Anchor(retry.GetComponent<RectTransform>(), new Vector2(0.25f, 0.33f), new Vector2(0.75f, 0.43f), Vector2.zero, Vector2.zero);
        }

        void BuildClearPanel(RectTransform root)
        {
            clearPanel = CreateOverlayPanel("Clear Panel", root);
            CreateText("Title", clearPanel.transform, "Stage Clear", 54, TextAnchor.MiddleCenter);
            Anchor((RectTransform)clearPanel.transform.Find("Title"), new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.74f), Vector2.zero, Vector2.zero);

            clearStatsText = CreateText("Stats", clearPanel.transform, "", 30, TextAnchor.MiddleCenter);
            Anchor(clearStatsText.rectTransform, new Vector2(0.1f, 0.43f), new Vector2(0.9f, 0.61f), Vector2.zero, Vector2.zero);

            var retry = CreateButton("Retry Button", clearPanel.transform, "Retry", stageManager.RetryStage);
            Anchor(retry.GetComponent<RectTransform>(), new Vector2(0.18f, 0.32f), new Vector2(0.47f, 0.42f), Vector2.zero, Vector2.zero);

            nextButton = CreateButton("Next Button", clearPanel.transform, "Next", HandleClearNext);
            Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(0.53f, 0.32f), new Vector2(0.82f, 0.42f), Vector2.zero, Vector2.zero);
        }

        static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel.GetComponent<RectTransform>();
        }

        static GameObject CreateOverlayPanel(string name, Transform parent)
        {
            var panel = CreatePanel(name, parent, new Color(0.01f, 0.015f, 0.03f, 0.82f));
            Anchor(panel, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return panel.gameObject;
        }

        static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(Text));
            textObject.transform.SetParent(parent, false);
            var label = textObject.GetComponent<Text>();
            label.text = text;
            label.font = GetDefaultFont();
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = new Color(0.84f, 0.96f, 1f, 1f);
            label.raycastTarget = false;
            return label;
        }

        static Slider CreateSlider(string name, Transform parent)
        {
            var sliderObject = new GameObject(name, typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            var background = CreatePanel("Background", sliderObject.transform, new Color(0.12f, 0.16f, 0.2f, 1f));
            Anchor(background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            Anchor(fillArea.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            var fill = CreatePanel("Fill", fillArea.transform, new Color(0.2f, 0.95f, 0.72f, 1f));
            Anchor(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.interactable = false;
            slider.fillRect = fill;
            slider.targetGraphic = fill.GetComponent<Image>();
            return slider;
        }

        static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(name, typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.08f, 0.18f, 0.24f, 0.92f);
            var button = buttonObject.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var text = CreateText("Text", buttonObject.transform, label, 32, TextAnchor.MiddleCenter);
            Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        static Button CreateHoldButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction<bool> onHeldChanged)
        {
            var button = CreateButton(name, parent, label, null);
            button.GetComponentInChildren<Text>().fontSize = 30;

            var trigger = button.gameObject.AddComponent<EventTrigger>();
            AddHoldEvent(trigger, EventTriggerType.PointerDown, () => onHeldChanged?.Invoke(true));
            AddHoldEvent(trigger, EventTriggerType.PointerUp, () => onHeldChanged?.Invoke(false));
            AddHoldEvent(trigger, EventTriggerType.PointerExit, () => onHeldChanged?.Invoke(false));
            AddHoldEvent(trigger, EventTriggerType.Cancel, () => onHeldChanged?.Invoke(false));
            return button;
        }

        static void AddHoldEvent(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction callback)
        {
            var entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(_ => callback.Invoke());
            trigger.triggers.Add(entry);
        }

        void UpdateFuelGauge(float normalizedFuel)
        {
            if (!Mathf.Approximately(displayedFuel, normalizedFuel))
            {
                fuelSlider.value = normalizedFuel;
                displayedFuel = normalizedFuel;
            }

            UpdateFuelColor(normalizedFuel);
        }

        void UpdateFuelColor(float normalizedFuel)
        {
            if (fuelFillImage == null)
            {
                return;
            }

            if (normalizedFuel > 0.2f)
            {
                fuelFillImage.color = fuelNormalColor;
                return;
            }

            var blink = Mathf.PingPong(Time.unscaledTime * 3.5f, 1f);
            fuelFillImage.color = Color.Lerp(fuelCriticalDimColor, fuelCriticalColor, blink);
        }

        void UpdateSpeedText(float speed)
        {
            var speedTenths = Mathf.RoundToInt(speed * 10f);
            if (speedTenths == displayedSpeedTenths)
            {
                return;
            }

            displayedSpeedTenths = speedTenths;
            speedText.text = $"{speedTenths * 0.1f:0.0} m/s";
        }

        void UpdateLives(int lives)
        {
            if (livesText == null || lives == displayedLives)
            {
                return;
            }

            displayedLives = lives;
            livesText.text = $"Lives: {lives}";
            livesText.color = lives <= 1 ? new Color(1f, 0.18f, 0.16f, 1f) : new Color(0.84f, 0.96f, 1f, 1f);
        }

        void UpdateScore(int score)
        {
            if (scoreText == null || score == displayedScore)
            {
                return;
            }

            displayedScore = score;
            scoreText.text = $"Score: {score}";
        }

        void HandleClearNext()
        {
            if (clearNextAction != null)
            {
                var action = clearNextAction;
                clearNextAction = null;
                action.Invoke();
                return;
            }

            stageManager.NextStage();
        }

        static string FormatScoreBreakdown(StageScoreBreakdown breakdown)
        {
            var text = $"Stage Clear: +{breakdown.stageClearScore}\nFuel Bonus: +{breakdown.fuelBonus}\n";
            if (breakdown.isFinalStage)
            {
                text += $"Lives Bonus: +{breakdown.livesBonus}\n";
            }

            text += $"Stage Total: +{breakdown.stageTotal}\n";
            text += breakdown.isFinalStage ? $"Final Score: {breakdown.currentScore}" : $"Current Score: {breakdown.currentScore}";
            return text;
        }

        void UpdateMarkerFeedback()
        {
            if (markerText == null || !markerFeedbackActive)
            {
                return;
            }

            if (Time.unscaledTime < markerFeedbackUntil)
            {
                markerText.color = Color.Lerp(markerLimitColor, markerLimitFlashColor, Mathf.PingPong(Time.unscaledTime * 12f, 1f));
                return;
            }

            markerFeedbackActive = false;
            markerText.color = MarkerTextBaseColor();
        }

        Color MarkerTextBaseColor()
        {
            return displayedMarkerMax >= 0 && displayedMarkerCount >= displayedMarkerMax ? markerLimitColor : markerNormalColor;
        }

        static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                DontDestroyOnLoad(eventSystem.gameObject);
            }

            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
            {
                if (module is InputSystemUIInputModule)
                {
                    continue;
                }

                UnityEngine.Object.Destroy(module);
            }

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            inputModule.AssignDefaultActions();
        }

        static Font GetDefaultFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
