using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal class SpriteGenerator
    {
        internal Sprite GenerateHexagonSprite(int size, Color hexColor)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2, size / 2);
            float radius = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new Vector2(x, y);
                    if (IsPointInHexagon(point, center, radius))
                    {
                        pixels[y * size + x] = hexColor;
                    }
                    else
                    {
                        pixels[y * size + x] = new Color(0, 0, 0, 0);
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Rect rect = new Rect(0, 0, size, size);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        internal Sprite GenerateWallSprite(int size, Color wallColor)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = wallColor;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private bool IsPointInHexagon(Vector2 point, Vector2 center, float radius)
        {
            int sides = 6;
            float angleStep = 2 * Mathf.PI / sides;
            Vector2[] vertices = new Vector2[sides];

            for (int i = 0; i < sides; i++)
            {
                float angle = i * angleStep;
                vertices[i] = new Vector2(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius);
            }

            int intersections = 0;
            for (int i = 0; i < sides; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % sides];
                if ((v1.y > point.y) != (v2.y > point.y) && point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x)
                {
                    intersections++;
                }
            }
            return intersections % 2 == 1;
        }
    }
}

