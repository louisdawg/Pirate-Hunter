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
    private static Sprite _circle;
    private static Sprite _torpedo;
    private static Sprite _mine;

    public static Sprite Wave => _wave ??= CreateWave();
    public static Sprite Island => _island ??= CreateIsland();
    public static Sprite Ring => _ring ??= CreateRing(64, 2.5f);
    public static Sprite Circle => _circle ??= CreateCircle(64);
    public static Sprite Torpedo => _torpedo ??= CreateTorpedo();
    public static Sprite Mine => _mine ??= CreateMine();

    private static Sprite CreateMine()
    {
        const int size = 64;
        var texture = NewTexture(size, size);
        var pixels = new Color32[size * size];

        var shell = new Color32(52, 62, 72, 255);
        var rim = new Color32(92, 104, 116, 255);
        var clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - size * 0.5f + 0.5f;
                float dy = y - size * 0.5f + 0.5f;
                float angle = Mathf.Atan2(dy, dx);
                float radius = Mathf.Sqrt(dx * dx + dy * dy);

                float spike = Mathf.Abs(Mathf.Cos(angle * 4f));
                float edge = 17f + 9f * spike * spike;

                if (radius <= edge)
                {
                    pixels[y * size + x] = radius > edge - 3f ? rim : shell;
                }
                else
                {
                    pixels[y * size + x] = clear;
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite CreateTorpedo()
    {
        const int width = 32;
        const int height = 64;
        var texture = NewTexture(width, height);
        var pixels = new Color32[width * height];

        var body = new Color32(70, 82, 94, 255);
        var highlight = new Color32(140, 155, 168, 255);
        var nose = new Color32(225, 85, 60, 255);
        var fin = new Color32(55, 65, 75, 255);
        var clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int halfWidth;
                if (y < 4 || y > 59)
                {
                    pixels[y * width + x] = clear;
                    continue;
                }

                if (y < 22)
                {
                    halfWidth = Mathf.RoundToInt(Mathf.Lerp(2f, 8f, (y - 4f) / 18f));
                }
                else if (y > 48)
                {
                    halfWidth = Mathf.RoundToInt(Mathf.Lerp(8f, 4f, (y - 48f) / 11f));
                }
                else
                {
                    halfWidth = 8;
                }

                if (Mathf.Abs(x - width * 0.5f) > halfWidth)
                {
                    pixels[y * width + x] = clear;
                    continue;
                }

                Color32 color = body;
                if (y < 22)
                {
                    color = nose;
                }
                else if (y > 48)
                {
                    color = fin;
                }
                else if (x > 10 && x < 14)
                {
                    color = highlight;
                }

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

    private static Sprite CreateCircle(int size)
    {
        var texture = NewTexture(size, size);
        var pixels = new Color32[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit);
    }

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
