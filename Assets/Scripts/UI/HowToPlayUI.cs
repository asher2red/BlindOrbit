using System;
using BlindOrbit.Gameplay;
using BlindOrbit.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace BlindOrbit.UI
{
    public sealed class HowToPlayUI : MonoBehaviour
    {
        static readonly Entry[] staticEntries =
        {
            new Entry(ObstacleKind.CircleAsteroid, "ASTEROID", "Solid rock. Any collision destroys the ship."),
            new Entry(ObstacleKind.EllipseAsteroid, "ELLIPSE", "A long rock that blocks narrow flight paths."),
            new Entry(ObstacleKind.LongWall, "LONG WALL", "A thin solid barrier. Watch its angle."),
            new Entry(ObstacleKind.HollowRing, "HOLLOW RING", "Solid rim with a safe opening through its center."),
            new Entry(ObstacleKind.MazeStructure, "MAZE", "Connected walls that create blind corridors.")
        };

        static readonly Entry[] deviceEntries =
        {
            new Entry(ObstacleKind.BlackHole, "BLACK HOLE", "Purple gravity field pulls the ship inward."),
            new Entry(ObstacleKind.WarpHole, "WARP HOLE", "Blue-violet gate teleports you to another point."),
            new Entry(ObstacleKind.OrbitingObstacle, "ORBITER", "Orange asteroid moves around a fixed center."),
            new Entry(ObstacleKind.RotatingObstacle, "ROTATING BAR", "Red warning bar continuously spins."),
            new Entry(ObstacleKind.Booster, "BOOSTER", "Green arrow launches the ship in its pointing direction."),
            new Entry(ObstacleKind.FuelDrain, "FUEL DRAIN", "Red-orange field drains fuel while you remain inside.")
        };

        RectTransform content;
        Text pageText;
        Button previousButton;
        Button nextButton;
        Action onClosed;
        int page;

        public void Show(Action closed)
        {
            onClosed = closed;
            Build();
            ShowPage(0);
        }

        void Build()
        {
            var canvas = ArcadeUIFactory.CreateCanvas("How To Play Canvas", transform);
            canvas.sortingOrder = 20;
            var overlay = ArcadeUIFactory.CreatePanel("Help Overlay", canvas.transform, new Color(0.006f, 0.012f, 0.026f, 0.985f));
            ArcadeUIFactory.Anchor(overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var frame = ArcadeUIFactory.CreatePanel("Frame", overlay, new Color(0.035f, 0.075f, 0.11f, 1f));
            ArcadeUIFactory.Anchor(frame, new Vector2(0.055f, 0.055f), new Vector2(0.945f, 0.945f), Vector2.zero, Vector2.zero);

            var inner = ArcadeUIFactory.CreatePanel("Inner", frame, new Color(0.009f, 0.018f, 0.04f, 1f));
            ArcadeUIFactory.Anchor(inner, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));

            var title = ArcadeUIFactory.CreateText("Title", inner, "HOW TO PLAY", 54, TextAnchor.MiddleCenter);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            pageText = ArcadeUIFactory.CreateText("Page", inner, string.Empty, 25, TextAnchor.MiddleCenter);
            pageText.color = new Color(0.38f, 0.85f, 1f, 1f);
            ArcadeUIFactory.Anchor(pageText.rectTransform, new Vector2(0.2f, 0.845f), new Vector2(0.8f, 0.895f), Vector2.zero, Vector2.zero);

            content = ArcadeUIFactory.CreatePanel("Content", inner, Color.clear);
            ArcadeUIFactory.Anchor(content, new Vector2(0.045f, 0.14f), new Vector2(0.955f, 0.84f), Vector2.zero, Vector2.zero);

            previousButton = ArcadeUIFactory.CreateButton("Previous", inner, "< PREV", PreviousPage);
            ArcadeUIFactory.Anchor(previousButton.GetComponent<RectTransform>(), new Vector2(0.05f, 0.045f), new Vector2(0.28f, 0.115f), Vector2.zero, Vector2.zero);

            var close = ArcadeUIFactory.CreateButton("Close", inner, "CLOSE", Close);
            ArcadeUIFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(0.36f, 0.045f), new Vector2(0.64f, 0.115f), Vector2.zero, Vector2.zero);

            nextButton = ArcadeUIFactory.CreateButton("Next", inner, "NEXT >", NextPage);
            ArcadeUIFactory.Anchor(nextButton.GetComponent<RectTransform>(), new Vector2(0.72f, 0.045f), new Vector2(0.95f, 0.115f), Vector2.zero, Vector2.zero);
        }

        void ShowPage(int newPage)
        {
            page = Mathf.Clamp(newPage, 0, 1);
            for (var i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            if (page == 0)
            {
                pageText.text = "1 / 2   CONTROLS & SOLID OBSTACLES";
                BuildControls();
                BuildEntries(staticEntries, 0.58f, 0.02f);
            }
            else
            {
                pageText.text = "2 / 2   SPACE DEVICES";
                BuildEntries(deviceEntries, 0.98f, 0.02f);
            }

            previousButton.interactable = page > 0;
            nextButton.interactable = page < 1;
        }

        void BuildControls()
        {
            var panel = ArcadeUIFactory.CreatePanel("Controls", content, new Color(0.04f, 0.1f, 0.14f, 0.88f));
            ArcadeUIFactory.Anchor(panel, new Vector2(0f, 0.64f), new Vector2(1f, 0.98f), Vector2.zero, Vector2.zero);

            var heading = ArcadeUIFactory.CreateText("Heading", panel, "FLIGHT CONTROLS", 30, TextAnchor.MiddleLeft);
            heading.color = new Color(0.2f, 0.95f, 0.72f, 1f);
            ArcadeUIFactory.Anchor(heading.rectTransform, new Vector2(0.04f, 0.7f), new Vector2(0.96f, 0.94f), Vector2.zero, Vector2.zero);

            var body = ArcadeUIFactory.CreateText("Body", panel,
                "A / LEFT     Rotate left\nD / RIGHT   Rotate right\nW / UP       Forward thrust\nDOWN         Place a marker\n\nMomentum remains after thrust. Reach the green goal and save fuel.",
                25, TextAnchor.UpperLeft);
            ArcadeUIFactory.Anchor(body.rectTransform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.72f), Vector2.zero, Vector2.zero);
        }

        void BuildEntries(Entry[] entries, float top, float bottom)
        {
            var height = (top - bottom) / entries.Length;
            for (var i = 0; i < entries.Length; i++)
            {
                var yMax = top - i * height;
                var yMin = yMax - height + 0.008f;
                BuildEntry(entries[i], yMin, yMax);
            }
        }

        void BuildEntry(Entry entry, float yMin, float yMax)
        {
            var row = ArcadeUIFactory.CreatePanel(entry.title, content, new Color(0.025f, 0.055f, 0.08f, 0.9f));
            ArcadeUIFactory.Anchor(row, new Vector2(0f, yMin), new Vector2(1f, yMax), Vector2.zero, Vector2.zero);
            CreateLegendIcon(row, entry.kind);

            var title = ArcadeUIFactory.CreateText("Name", row, entry.title, 27, TextAnchor.LowerLeft);
            title.color = LegendColor(entry.kind);
            ArcadeUIFactory.Anchor(title.rectTransform, new Vector2(0.2f, 0.48f), new Vector2(0.96f, 0.9f), Vector2.zero, Vector2.zero);

            var description = ArcadeUIFactory.CreateText("Description", row, entry.description, 22, TextAnchor.UpperLeft);
            description.color = new Color(0.76f, 0.86f, 0.9f, 1f);
            ArcadeUIFactory.Anchor(description.rectTransform, new Vector2(0.2f, 0.08f), new Vector2(0.96f, 0.5f), Vector2.zero, Vector2.zero);
        }

        static void CreateLegendIcon(Transform parent, ObstacleKind kind)
        {
            var color = LegendColor(kind);
            var icon = ArcadeUIFactory.CreatePanel("Icon", parent, color);
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = UsesCircularIcon(kind) ? PlaceholderSpriteFactory.Circle() : PlaceholderSpriteFactory.Square();
            var min = new Vector2(0.045f, 0.2f);
            var max = new Vector2(0.155f, 0.8f);
            if (kind == ObstacleKind.LongWall || kind == ObstacleKind.RotatingObstacle)
            {
                min = new Vector2(0.025f, 0.43f);
                max = new Vector2(0.175f, 0.57f);
            }
            else if (kind == ObstacleKind.EllipseAsteroid)
            {
                min = new Vector2(0.075f, 0.12f);
                max = new Vector2(0.125f, 0.88f);
            }

            ArcadeUIFactory.Anchor(icon, min, max, Vector2.zero, Vector2.zero);

            var glyph = Glyph(kind);
            if (!string.IsNullOrEmpty(glyph))
            {
                var mark = ArcadeUIFactory.CreateText("Glyph", icon, glyph, 30, TextAnchor.MiddleCenter);
                mark.color = new Color(0.01f, 0.025f, 0.04f, 0.95f);
                ArcadeUIFactory.Anchor(mark.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
        }

        static string Glyph(ObstacleKind kind)
        {
            switch (kind)
            {
                case ObstacleKind.HollowRing: return "O";
                case ObstacleKind.MazeStructure: return "#";
                case ObstacleKind.BlackHole: return "IN";
                case ObstacleKind.WarpHole: return "W";
                case ObstacleKind.OrbitingObstacle: return "ORB";
                case ObstacleKind.RotatingObstacle: return "!";
                case ObstacleKind.Booster: return "UP";
                case ObstacleKind.FuelDrain: return "-F";
                default: return string.Empty;
            }
        }

        static bool UsesCircularIcon(ObstacleKind kind)
        {
            return kind == ObstacleKind.CircleAsteroid ||
                   kind == ObstacleKind.EllipseAsteroid ||
                   kind == ObstacleKind.HollowRing ||
                   kind == ObstacleKind.BlackHole ||
                   kind == ObstacleKind.WarpHole ||
                   kind == ObstacleKind.OrbitingObstacle ||
                   kind == ObstacleKind.FuelDrain;
        }

        static Color LegendColor(ObstacleKind kind)
        {
            switch (kind)
            {
                case ObstacleKind.BlackHole: return new Color(0.68f, 0.2f, 1f, 1f);
                case ObstacleKind.WarpHole: return new Color(0.15f, 0.82f, 1f, 1f);
                case ObstacleKind.OrbitingObstacle: return new Color(1f, 0.58f, 0.18f, 1f);
                case ObstacleKind.RotatingObstacle: return new Color(1f, 0.22f, 0.26f, 1f);
                case ObstacleKind.Booster: return new Color(0.12f, 1f, 0.7f, 1f);
                case ObstacleKind.FuelDrain: return new Color(1f, 0.3f, 0.12f, 1f);
                default: return new Color(0.56f, 0.66f, 0.76f, 1f);
            }
        }

        void PreviousPage() => ShowPage(page - 1);
        void NextPage() => ShowPage(page + 1);

        void Close()
        {
            onClosed?.Invoke();
            Destroy(gameObject);
        }

        readonly struct Entry
        {
            public readonly ObstacleKind kind;
            public readonly string title;
            public readonly string description;

            public Entry(ObstacleKind kind, string title, string description)
            {
                this.kind = kind;
                this.title = title;
                this.description = description;
            }
        }
    }
}
