using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BlindOrbit.UI
{
    public sealed class NameEntryUI : MonoBehaviour
    {
        const int MaxNameLength = 3;

        InputField inputField;
        Text remainingText;
        Button submitButton;
        Action<string> onSubmit;
        bool submitted;

        public void Show(int score, Action<string> submitCallback)
        {
            onSubmit = submitCallback;
            Build(score);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (!submitted && keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                Submit();
            }
        }

        void Build(int score)
        {
            var canvas = ArcadeUIFactory.CreateCanvas("Name Entry UI", transform);
            var root = canvas.GetComponent<RectTransform>();

            var overlay = ArcadeUIFactory.CreatePanel("Overlay", root, new Color(0.01f, 0.015f, 0.03f, 0.9f));
            ArcadeUIFactory.Anchor(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = ArcadeUIFactory.CreateText("Title", overlay, "ENTER INITIALS", 58, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.78f), Vector2.zero, Vector2.zero);

            var scoreText = ArcadeUIFactory.CreateText("Score", overlay, $"Score: {score:000000}", 36, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(scoreText.rectTransform, new Vector2(0.08f, 0.61f), new Vector2(0.92f, 0.67f), Vector2.zero, Vector2.zero);

            inputField = CreateInputField(overlay);
            ArcadeUIFactory.Anchor(inputField.GetComponent<RectTransform>(), new Vector2(0.28f, 0.48f), new Vector2(0.72f, 0.58f), Vector2.zero, Vector2.zero);
            inputField.onValueChanged.AddListener(HandleNameChanged);
            inputField.onEndEdit.AddListener(_ => UpdateSubmitButton());

            remainingText = ArcadeUIFactory.CreateText("Remaining", overlay, "3 characters remaining", 28, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(remainingText.rectTransform, new Vector2(0.1f, 0.41f), new Vector2(0.9f, 0.47f), Vector2.zero, Vector2.zero);

            submitButton = ArcadeUIFactory.CreateButton("Submit Button", overlay, "Submit", Submit);
            ArcadeUIFactory.Anchor(submitButton.GetComponent<RectTransform>(), new Vector2(0.25f, 0.31f), new Vector2(0.75f, 0.39f), Vector2.zero, Vector2.zero);
            UpdateSubmitButton();

            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        InputField CreateInputField(Transform parent)
        {
            var fieldObject = new GameObject("Name Input", typeof(Image), typeof(InputField));
            fieldObject.transform.SetParent(parent, false);
            fieldObject.GetComponent<Image>().color = new Color(0.08f, 0.18f, 0.24f, 0.96f);

            var text = ArcadeUIFactory.CreateText("Text", fieldObject.transform, string.Empty, 56, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 0f), new Vector2(-14f, 0f));

            var placeholder = ArcadeUIFactory.CreateText("Placeholder", fieldObject.transform, "AAA", 56, TextAnchor.MiddleCenter);
            placeholder.color = new Color(0.35f, 0.48f, 0.58f, 1f);
            ArcadeUIFactory.Anchor(placeholder.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 0f), new Vector2(-14f, 0f));

            var field = fieldObject.GetComponent<InputField>();
            field.textComponent = text;
            field.placeholder = placeholder;
            field.characterLimit = MaxNameLength;
            field.contentType = InputField.ContentType.Alphanumeric;
            field.caretColor = Color.white;
            field.selectionColor = new Color(0.3f, 0.75f, 1f, 0.35f);
            return field;
        }

        void HandleNameChanged(string value)
        {
            var sanitized = Sanitize(value);
            if (inputField.text != sanitized)
            {
                inputField.SetTextWithoutNotify(sanitized);
            }

            var remaining = Mathf.Max(0, MaxNameLength - sanitized.Length);
            remainingText.text = $"{remaining} characters remaining";
            UpdateSubmitButton();
        }

        static string Sanitize(string value)
        {
            var result = string.Empty;
            for (var i = 0; i < value.Length && result.Length < MaxNameLength; i++)
            {
                var character = char.ToUpperInvariant(value[i]);
                if (char.IsLetterOrDigit(character))
                {
                    result += character;
                }
            }

            return result;
        }

        void UpdateSubmitButton()
        {
            if (submitButton != null)
            {
                submitButton.interactable = !string.IsNullOrEmpty(inputField.text);
            }
        }

        void Submit()
        {
            if (submitted || inputField == null || string.IsNullOrEmpty(inputField.text))
            {
                return;
            }

            submitted = true;
            onSubmit?.Invoke(inputField.text);
        }
    }
}
