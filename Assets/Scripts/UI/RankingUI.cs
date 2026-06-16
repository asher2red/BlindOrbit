using System;
using System.Collections.Generic;
using BlindOrbit.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace BlindOrbit.UI
{
    public sealed class RankingUI : MonoBehaviour
    {
        const float ReturnDelay = 60f;
        const int VisibleRankingCount = 10;

        Text countdownText;
        Action onDone;
        float remainingSeconds;
        bool closing;

        public void Show(IReadOnlyList<RankingEntry> entries, int playerRank, int score, Action doneCallback)
        {
            onDone = doneCallback;
            remainingSeconds = ReturnDelay;
            Build(entries, playerRank, score);
        }

        void Update()
        {
            if (closing)
            {
                return;
            }

            remainingSeconds -= Time.unscaledDeltaTime;
            var displaySeconds = Mathf.Max(0, Mathf.CeilToInt(remainingSeconds));
            countdownText.text = $"Returning to title in: {displaySeconds}";
            if (remainingSeconds <= 0f)
            {
                Close();
            }
        }

        void Build(IReadOnlyList<RankingEntry> entries, int playerRank, int score)
        {
            var canvas = ArcadeUIFactory.CreateCanvas("Ranking UI", transform);
            var root = canvas.GetComponent<RectTransform>();

            var overlay = ArcadeUIFactory.CreatePanel("Overlay", root, new Color(0.01f, 0.015f, 0.03f, 0.92f));
            ArcadeUIFactory.Anchor(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = ArcadeUIFactory.CreateText("Title", overlay, "RANKING", 62, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);

            var result = ArcadeUIFactory.CreateText("Result", overlay, $"Your Rank: {playerRank}\nScore: {score:000000}", 34, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(result.rectTransform, new Vector2(0.08f, 0.73f), new Vector2(0.92f, 0.83f), Vector2.zero, Vector2.zero);

            var rankingText = ArcadeUIFactory.CreateText("Ranking List", overlay, BuildRankingText(entries), 30, TextAnchor.UpperLeft);
            ArcadeUIFactory.Anchor(rankingText.rectTransform, new Vector2(0.16f, 0.28f), new Vector2(0.84f, 0.7f), Vector2.zero, Vector2.zero);

            countdownText = ArcadeUIFactory.CreateText("Countdown", overlay, "Returning to title in: 60", 28, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(countdownText.rectTransform, new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.24f), Vector2.zero, Vector2.zero);

            var next = ArcadeUIFactory.CreateButton("Next Button", overlay, "Next", Close);
            ArcadeUIFactory.Anchor(next.GetComponent<RectTransform>(), new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.16f), Vector2.zero, Vector2.zero);
        }

        static string BuildRankingText(IReadOnlyList<RankingEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return "No rankings yet";
            }

            var text = string.Empty;
            var count = Mathf.Min(entries.Count, VisibleRankingCount);
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i];
                text += $"{i + 1,2}. {entry.name,-3}  {entry.score:000000}  {entry.date}\n";
            }

            return text;
        }

        void Close()
        {
            if (closing)
            {
                return;
            }

            closing = true;
            onDone?.Invoke();
        }
    }
}
