using UnityEngine;

/// Procedural sprite and audio helpers so the project ships zero binary assets.
public static class Art2D
{
    public static Sprite SolidCircle(Color color, int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - r + 0.5f, dy = y - r + 0.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            // Soft edge — anti-aliased disc.
            float a = Mathf.Clamp01(r - d);
            tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite Triangle(Color color, int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // Up-pointing isoceles triangle.
            float u = (float)x / size, v = (float)y / size;
            float halfWidth = (1f - v) * 0.5f;
            float center = 0.5f;
            bool inside = v <= 1f && u >= center - halfWidth && u <= center + halfWidth;
            tex.SetPixel(x, y, inside ? color : Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.25f), 100f);
    }

    public static Sprite Diamond(Color color, int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - r + 0.5f), dy = Mathf.Abs(y - r + 0.5f);
            float d = dx + dy;                        // L1 — diamond
            float a = Mathf.Clamp01(r - d);
            tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * a));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite Square(Color color, int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    // ───────────────────────── Audio ─────────────────────────
    public static AudioClip Tone(float freq, float duration, float decay = 14f)
    {
        int sr = 44100;
        int n = (int)(sr * duration);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Exp(-t * decay);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        }
        var c = AudioClip.Create("tone_" + freq, n, 1, sr, false);
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Noise(float duration, float decay = 10f)
    {
        int sr = 44100;
        int n = (int)(sr * duration);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Exp(-t * decay);
            d[i] = (Random.value * 2f - 1f) * env * 0.5f;
        }
        var c = AudioClip.Create("noise_" + duration, n, 1, sr, false);
        c.SetData(d, 0);
        return c;
    }

    public static AudioClip Chime(float baseFreq, float duration)
    {
        int sr = 44100;
        int n = (int)(sr * duration);
        var d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / sr;
            float env = Mathf.Exp(-t * 6f);
            d[i] = (Mathf.Sin(2 * Mathf.PI * baseFreq * t) * 0.4f
                  + Mathf.Sin(2 * Mathf.PI * baseFreq * 1.5f * t) * 0.3f
                  + Mathf.Sin(2 * Mathf.PI * baseFreq * 2f * t) * 0.2f) * env;
        }
        var c = AudioClip.Create("chime_" + baseFreq, n, 1, sr, false);
        c.SetData(d, 0);
        return c;
    }
}
