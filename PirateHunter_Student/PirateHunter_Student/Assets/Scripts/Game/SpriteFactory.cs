using UnityEngine;

/// <summary>
/// Erzeugt die wenigen selbst gezeichneten Sprites (Wellen, Inseln, Ringe).
/// So muss das Projekt nicht um zusätzliche Bilddateien erweitert werden.
/// </summary>
public static class SpriteFactory
{
    private const float PixelsPerUnit = 64f;

    private static Sprite _wave;
    private static Sprite _island;
    private static Sprite _ring;

    public static Sprite Wave => _wave ??= CreateWave();
    public static Sprite Island => _island ??= CreateIsland();
    public static Sprite Ring => _ring ??= CreateRing(64, 2.5f);

    private static Sprite CreateWave()
    {
        const int width = 96;
        const int height = 12;
        var texture = NewTexture(width, height);
        var pixels = new Color32[width * height];

        for (int x = 0; x < width; x++)
        {
            float t = x / (width - 1f);
            float curve = Mathf.Sin(t * Mathf.PI * 2f) * 2f;
            float fade = Mathf.Sin(t * Mathf.PI);

            for (int y = 0; y < height; y++)
            {
                float distance = Mathf.Abs(y - (height * 0.5f + curve));
                byte alpha = (byte)(Mathf.Clamp01(1f - distance / 1.6f) * 255f * fade);
                pixels[y * width + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite CreateIsland()
    {
        const int size = 128;
        var texture = NewTexture(size, size);
        var pixels = new Color32[size * size];

        var sand = new Color32(233, 214, 166, 255);
        var sandDark = new Color32(206, 183, 132, 255);
        var grass = new Color32(92, 178, 105, 255);
        var grassDark = new Color32(64, 148, 82, 255);
        var clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size * 0.5f + 0.5f;
                float dy = y - size * 0.5f + 0.5f;
                float angle = Mathf.Atan2(dy, dx);
                float radius = Mathf.Sqrt(dx * dx + dy * dy);

                float edge = 54f
                    + 4f * Mathf.Sin(angle * 3f + 1.2f)
                    + 2.5f * Mathf.Sin(angle * 7f + 0.4f);

                Color32 color;
                if (radius > edge)
                {
                    color = clear;
                }
                else if (radius < edge - 10f - 2f * Mathf.Sin(angle * 5f))
                {
                    float noise = Mathf.PerlinNoise(x * 0.13f, y * 0.13f);
                    color = noise > 0.56f ? grassDark : grass;
                }
                else
                {
                    color = radius > edge - 3f ? sandDark : sand;
                }

                pixels[y * size + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite CreateRing(int size, float thickness)
    {
        var texture = NewTexture(size, size);
        var pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(1f - Mathf.Abs(distance - radius) / thickness);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Texture2D NewTexture(int width, int height)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
    }
}
