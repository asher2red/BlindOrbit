using System.Collections.Generic;
using UnityEngine;

namespace BlindOrbit.Utility
{
    public static class PlaceholderSpriteFactory
    {
        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Square()
        {
            return CreateSolid("square", 16, 16, Color.white);
        }

        public static Sprite Circle()
        {
            const string key = "circle";
            if (Cache.TryGetValue(key, out var sprite))
            {
                return sprite;
            }

            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.47f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = Mathf.Clamp01(radius - distance + 1f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            Cache[key] = sprite;
            return sprite;
        }

        public static Sprite Triangle()
        {
            const string key = "triangle";
            if (Cache.TryGetValue(key, out var sprite))
            {
                return sprite;
            }

            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var top = new Vector2(size * 0.5f, size * 0.92f);
            var left = new Vector2(size * 0.15f, size * 0.12f);
            var right = new Vector2(size * 0.85f, size * 0.12f);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var p = new Vector2(x, y);
                    texture.SetPixel(x, y, IsInsideTriangle(p, top, left, right) ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            Cache[key] = sprite;
            return sprite;
        }

        static Sprite CreateSolid(string key, int width, int height, Color color)
        {
            if (Cache.TryGetValue(key, out var sprite))
            {
                return sprite;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
            Cache[key] = sprite;
            return sprite;
        }

        static bool IsInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            var d1 = Sign(point, a, b);
            var d2 = Sign(point, b, c);
            var d3 = Sign(point, c, a);
            var hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
            var hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNegative && hasPositive);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}
