using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BlindOrbit.UI
{
    public static class ArcadeUIFactory
    {
        public static Canvas CreateCanvas(string name, Transform parent = null)
        {
            EnsureEventSystem();
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null)
            {
                canvasObject.transform.SetParent(parent, false);
            }

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;
            return canvas;
        }

        public static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel.GetComponent<RectTransform>();
        }

        public static Text CreateText(string name, Transform parent, string text, int fontSize, TextAnchor alignment)
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

        public static Button CreateButton(string name, Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(name, typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = new Color(0.08f, 0.18f, 0.24f, 0.94f);
            var button = buttonObject.GetComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var text = CreateText("Text", buttonObject.transform, label, 32, TextAnchor.MiddleCenter);
            Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static void EnsureEventSystem()
        {
            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                Object.DontDestroyOnLoad(eventSystem.gameObject);
            }

            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
            {
                if (module is InputSystemUIInputModule)
                {
                    continue;
                }

                Object.Destroy(module);
            }

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            inputModule.AssignDefaultActions();
        }

        public static Font GetDefaultFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}
