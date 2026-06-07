using System.IO;
using UnityEngine;

/// Procedural sprite and audio helpers so the project ships zero binary assets.
public static class Art2D
{
    public static Sprite FromPngFile(string relativePath, float pixelsPerUnit = 100f, FilterMode filter = FilterMode.Point)
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
        if (!File.Exists(fullPath)) return null;

        byte[] pngBytes = File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(pngBytes)) return null;
        tex.filterMode = filter;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

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

    public static Sprite SoftRectangle(Color center, Color edge, int width = 96, int height = 96)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (x + 0.5f) / width;
            float v = (y + 0.5f) / height;
            float edgeDistance = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v)) * 2f;
            float shade = Mathf.Clamp01(edgeDistance);
            tex.SetPixel(x, y, Color.Lerp(edge, center, shade));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite Projectile(int width = 96, int height = 32)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float nx = Mathf.Abs((x - center.x) / (width * 0.5f));
            float ny = Mathf.Abs((y - center.y) / (height * 0.5f));
            float core = Mathf.Clamp01(1f - Mathf.Pow(nx, 2.2f) - Mathf.Pow(ny, 2.8f));
            float glow = Mathf.Clamp01(1f - nx * 0.8f - ny * 1.6f);
            Color color = Color.Lerp(new Color(1f, 0.45f, 0.08f, 0f), new Color(1f, 0.78f, 0.18f, 0.65f), glow);
            color = Color.Lerp(color, new Color(1f, 0.96f, 0.55f, 1f), core);
            color.a *= Mathf.Max(core, glow * 0.65f);
            tex.SetPixel(x, y, color);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite Key(int size = 96)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color gold = new Color(1f, 0.74f, 0.14f, 1f);
        Color bright = new Color(1f, 0.95f, 0.45f, 1f);
        Color dark = new Color(0.64f, 0.32f, 0.05f, 1f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 p = new Vector2(x, y);
            float ring = Mathf.Abs(Vector2.Distance(p, new Vector2(size * 0.33f, size * 0.58f)) - size * 0.16f);
            bool bow = ring < size * 0.055f;
            bool shaft = x > size * 0.42f && x < size * 0.78f && Mathf.Abs(y - size * 0.55f) < size * 0.045f;
            bool toothA = x > size * 0.68f && x < size * 0.76f && y > size * 0.38f && y < size * 0.56f;
            bool toothB = x > size * 0.78f && x < size * 0.86f && y > size * 0.45f && y < size * 0.56f;
            if (bow || shaft || toothA || toothB)
            {
                float highlight = Mathf.Clamp01((float)y / size + (1f - (float)x / size) * 0.45f);
                tex.SetPixel(x, y, Color.Lerp(dark, Color.Lerp(gold, bright, highlight), 0.84f));
            }
            else tex.SetPixel(x, y, Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite ExitGate(int width = 96, int height = 128)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color frame = new Color(0.18f, 0.25f, 0.36f, 1f);
        Color frameLight = new Color(0.48f, 0.63f, 0.78f, 1f);
        Color portal = new Color(0.12f, 0.78f, 1f, 1f);
        Color portalDark = new Color(0.04f, 0.14f, 0.26f, 1f);

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width;
            float v = (float)y / height;
            bool outer = x < width * 0.12f || x > width * 0.88f || y < height * 0.08f || y > height * 0.9f;
            bool inner = x > width * 0.22f && x < width * 0.78f && y > height * 0.16f && y < height * 0.78f;
            if (outer)
            {
                tex.SetPixel(x, y, Color.Lerp(frame, frameLight, Mathf.Clamp01(v * 0.7f + (1f - u) * 0.2f)));
            }
            else if (inner)
            {
                float swirl = Mathf.Sin((u * 7f + v * 5f) * Mathf.PI) * 0.5f + 0.5f;
                tex.SetPixel(x, y, Color.Lerp(portalDark, portal, Mathf.Clamp01(v * 0.6f + swirl * 0.35f)));
            }
            else tex.SetPixel(x, y, new Color(0.03f, 0.05f, 0.08f, 0.72f));
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite WeaponUpgradeIcon(WeaponUpgradeKind upgrade, int size = 96)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = Color.clear;
        Color gold = new Color(1f, 0.82f, 0.18f, 1f);
        Color orange = new Color(1f, 0.36f, 0.08f, 1f);
        Color cyan = new Color(0.35f, 0.9f, 1f, 1f);
        Color blue = new Color(0.26f, 0.52f, 1f, 1f);
        Color white = new Color(1f, 0.96f, 0.72f, 1f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            Color pixel = clear;

            switch (upgrade)
            {
                case WeaponUpgradeKind.ExtraProjectile:
                    if (Capsule(u, v, 0.2f, 0.62f, 0.72f, 0.62f, 0.055f) ||
                        Capsule(u, v, 0.16f, 0.48f, 0.68f, 0.48f, 0.055f) ||
                        Capsule(u, v, 0.2f, 0.34f, 0.72f, 0.34f, 0.055f))
                        pixel = Color.Lerp(orange, white, u);
                    break;
                case WeaponUpgradeKind.RapidFire:
                    if (Capsule(u, v, 0.15f, 0.68f, 0.82f, 0.68f, 0.035f) ||
                        Capsule(u, v, 0.08f, 0.5f, 0.78f, 0.5f, 0.035f) ||
                        Capsule(u, v, 0.2f, 0.32f, 0.88f, 0.32f, 0.035f))
                        pixel = Color.Lerp(gold, white, u);
                    break;
                case WeaponUpgradeKind.DamageUp:
                    if (Mathf.Abs(u - 0.5f) + Mathf.Abs(v - 0.5f) < 0.3f)
                        pixel = Color.Lerp(orange, gold, v);
                    if (Mathf.Abs(u - 0.5f) < 0.055f || Mathf.Abs(v - 0.5f) < 0.055f)
                        pixel = white;
                    break;
                case WeaponUpgradeKind.PiercingShot:
                    if (Capsule(u, v, 0.12f, 0.5f, 0.72f, 0.5f, 0.055f) ||
                        TriangleRight(u, v, 0.68f, 0.5f, 0.18f, 0.24f))
                        pixel = Color.Lerp(cyan, white, u);
                    break;
                case WeaponUpgradeKind.BurnShot:
                    if (Mathf.Pow((u - 0.5f) / 0.22f, 2f) + Mathf.Pow((v - 0.38f) / 0.32f, 2f) < 1f ||
                        TriangleUp(u, v, 0.5f, 0.74f, 0.42f, 0.52f))
                        pixel = Color.Lerp(orange, gold, v);
                    break;
                case WeaponUpgradeKind.SlowShot:
                    if (Capsule(u, v, 0.22f, 0.5f, 0.78f, 0.5f, 0.028f) ||
                        Capsule(u, v, 0.5f, 0.22f, 0.5f, 0.78f, 0.028f) ||
                        Capsule(u, v, 0.3f, 0.3f, 0.7f, 0.7f, 0.028f) ||
                        Capsule(u, v, 0.7f, 0.3f, 0.3f, 0.7f, 0.028f))
                        pixel = Color.Lerp(blue, cyan, v);
                    break;
                case WeaponUpgradeKind.ExplosiveShot:
                    float burst = Mathf.Abs(u - 0.5f) + Mathf.Abs(v - 0.5f);
                    if (burst < 0.36f && Mathf.Sin(Mathf.Atan2(v - 0.5f, u - 0.5f) * 6f) + 1.5f > burst * 5f)
                        pixel = Color.Lerp(orange, gold, 1f - burst);
                    break;
            }

            tex.SetPixel(x, y, pixel);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite PassiveUpgradeIcon(PassiveUpgradeKind upgrade, int size = 96)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = Color.clear;
        Color green = new Color(0.5f, 1f, 0.68f, 1f);
        Color cyan = new Color(0.42f, 0.92f, 1f, 1f);
        Color red = new Color(1f, 0.28f, 0.32f, 1f);
        Color gold = new Color(1f, 0.82f, 0.18f, 1f);
        Color white = new Color(0.92f, 1f, 0.92f, 1f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            Color pixel = clear;

            switch (upgrade)
            {
                case PassiveUpgradeKind.MaxLivesUp:
                    bool left = Mathf.Pow((u - 0.38f) / 0.15f, 2f) + Mathf.Pow((v - 0.6f) / 0.15f, 2f) < 1f;
                    bool right = Mathf.Pow((u - 0.62f) / 0.15f, 2f) + Mathf.Pow((v - 0.6f) / 0.15f, 2f) < 1f;
                    if (left || right || TriangleDown(u, v, 0.5f, 0.28f, 0.48f, 0.42f)) pixel = red;
                    break;
                case PassiveUpgradeKind.MoveSpeedUp:
                    if (Capsule(u, v, 0.28f, 0.34f, 0.68f, 0.34f, 0.07f) ||
                        Capsule(u, v, 0.42f, 0.34f, 0.62f, 0.65f, 0.055f) ||
                        Capsule(u, v, 0.36f, 0.62f, 0.72f, 0.62f, 0.045f))
                        pixel = Color.Lerp(green, white, u);
                    break;
                case PassiveUpgradeKind.FireCooldownBonus:
                    if (TriangleDown(u, v, 0.48f, 0.2f, 0.34f, 0.38f) ||
                        TriangleUp(u, v, 0.52f, 0.8f, 0.34f, 0.48f))
                        pixel = Color.Lerp(cyan, white, v);
                    break;
                case PassiveUpgradeKind.XPBonus:
                    float a = Mathf.Atan2(v - 0.5f, u - 0.5f);
                    float r = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
                    float star = 0.24f + 0.08f * Mathf.Sin(a * 5f);
                    if (r < star) pixel = Color.Lerp(gold, white, 1f - r * 2.6f);
                    break;
            }

            tex.SetPixel(x, y, pixel);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite EnemySprite(EnemyKind kind, int size = 96)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        EnemyPalette(kind, out Color body, out Color shade, out Color accent, out Color eye);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            Color pixel = Color.clear;

            bool main = false;
            bool detail = false;
            bool eyes = false;

            switch (kind)
            {
                case EnemyKind.SlimeScout:
                    main = Ellipse(u, v, 0.5f, 0.4f, 0.34f, 0.24f) || Ellipse(u, v, 0.5f, 0.58f, 0.27f, 0.2f);
                    detail = v < 0.32f && main;
                    eyes = EyePair(u, v, 0.42f, 0.55f, 0.58f, 0.55f, 0.035f);
                    break;
                case EnemyKind.TinyBat:
                    main = Ellipse(u, v, 0.5f, 0.5f, 0.18f, 0.2f) ||
                        TriangleLeft(u, v, 0.28f, 0.5f, 0.34f, 0.24f) ||
                        TriangleRight(u, v, 0.72f, 0.5f, 0.34f, 0.24f);
                    detail = TriangleUp(u, v, 0.5f, 0.74f, 0.18f, 0.18f);
                    eyes = EyePair(u, v, 0.45f, 0.54f, 0.55f, 0.54f, 0.026f);
                    break;
                case EnemyKind.ShieldGuard:
                    main = RoundedBox(u, v, 0.5f, 0.47f, 0.27f, 0.32f) || TriangleUp(u, v, 0.5f, 0.77f, 0.3f, 0.22f);
                    detail = RoundedBox(u, v, 0.5f, 0.48f, 0.19f, 0.23f);
                    eyes = EyePair(u, v, 0.43f, 0.58f, 0.57f, 0.58f, 0.025f);
                    break;
                case EnemyKind.SparkSpitter:
                    main = Ellipse(u, v, 0.5f, 0.49f, 0.27f, 0.28f);
                    detail = TriangleRight(u, v, 0.69f, 0.52f, 0.2f, 0.18f) || Star(u, v, 0.34f, 0.72f, 0.13f);
                    eyes = EyePair(u, v, 0.43f, 0.55f, 0.55f, 0.55f, 0.025f);
                    break;
                case EnemyKind.BombSprite:
                    main = Ellipse(u, v, 0.5f, 0.42f, 0.3f, 0.28f);
                    detail = Capsule(u, v, 0.48f, 0.7f, 0.62f, 0.86f, 0.035f) || Star(u, v, 0.66f, 0.86f, 0.08f);
                    eyes = EyePair(u, v, 0.42f, 0.47f, 0.58f, 0.47f, 0.03f);
                    break;
                case EnemyKind.FrostWisp:
                    main = Ellipse(u, v, 0.5f, 0.52f, 0.25f, 0.3f) || TriangleDown(u, v, 0.5f, 0.2f, 0.22f, 0.22f);
                    detail = Capsule(u, v, 0.32f, 0.52f, 0.68f, 0.52f, 0.025f) || Capsule(u, v, 0.5f, 0.34f, 0.5f, 0.74f, 0.025f);
                    eyes = EyePair(u, v, 0.44f, 0.58f, 0.56f, 0.58f, 0.023f);
                    break;
                case EnemyKind.DashImp:
                    main = Ellipse(u, v, 0.5f, 0.45f, 0.24f, 0.27f) ||
                        TriangleUp(u, v, 0.33f, 0.75f, 0.15f, 0.2f) ||
                        TriangleUp(u, v, 0.67f, 0.75f, 0.15f, 0.2f);
                    detail = Capsule(u, v, 0.24f, 0.34f, 0.76f, 0.34f, 0.035f);
                    eyes = EyePair(u, v, 0.43f, 0.52f, 0.57f, 0.52f, 0.028f);
                    break;
                case EnemyKind.HealerFairy:
                    main = Ellipse(u, v, 0.5f, 0.47f, 0.18f, 0.23f) ||
                        Ellipse(u, v, 0.3f, 0.58f, 0.14f, 0.2f) ||
                        Ellipse(u, v, 0.7f, 0.58f, 0.14f, 0.2f);
                    detail = Capsule(u, v, 0.5f, 0.35f, 0.5f, 0.62f, 0.025f) || Capsule(u, v, 0.39f, 0.49f, 0.61f, 0.49f, 0.025f);
                    eyes = EyePair(u, v, 0.45f, 0.53f, 0.55f, 0.53f, 0.02f);
                    break;
                case EnemyKind.SummonerShade:
                    main = Ellipse(u, v, 0.5f, 0.5f, 0.26f, 0.32f) || TriangleDown(u, v, 0.5f, 0.18f, 0.28f, 0.28f);
                    detail = Capsule(u, v, 0.28f, 0.7f, 0.72f, 0.7f, 0.025f);
                    eyes = EyePair(u, v, 0.43f, 0.56f, 0.57f, 0.56f, 0.027f);
                    break;
                case EnemyKind.CrystalBrute:
                    main = Mathf.Abs(u - 0.5f) + Mathf.Abs(v - 0.5f) < 0.38f ||
                        TriangleUp(u, v, 0.34f, 0.78f, 0.16f, 0.18f) ||
                        TriangleUp(u, v, 0.66f, 0.78f, 0.16f, 0.18f);
                    detail = Mathf.Abs(u - 0.5f) < 0.045f || Mathf.Abs(v - 0.5f) < 0.045f;
                    eyes = EyePair(u, v, 0.43f, 0.58f, 0.57f, 0.58f, 0.026f);
                    break;
            }

            if (main)
            {
                float highlight = Mathf.Clamp01(v * 0.75f + (1f - u) * 0.18f);
                pixel = Color.Lerp(shade, body, highlight);
            }

            if (detail) pixel = Color.Lerp(pixel.a > 0f ? pixel : shade, accent, 0.85f);
            if (eyes) pixel = eye;

            tex.SetPixel(x, y, pixel);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100f);
    }

    static bool Capsule(float u, float v, float ax, float ay, float bx, float by, float radius)
    {
        Vector2 p = new Vector2(u, v);
        Vector2 a = new Vector2(ax, ay);
        Vector2 b = new Vector2(bx, by);
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
        return Vector2.Distance(p, a + ab * t) < radius;
    }

    static void EnemyPalette(EnemyKind kind, out Color body, out Color shade, out Color accent, out Color eye)
    {
        eye = new Color(0.07f, 0.06f, 0.09f, 1f);
        switch (kind)
        {
            case EnemyKind.TinyBat:
                body = new Color(0.52f, 0.42f, 0.9f, 1f);
                shade = new Color(0.26f, 0.2f, 0.48f, 1f);
                accent = new Color(0.88f, 0.76f, 1f, 1f);
                break;
            case EnemyKind.ShieldGuard:
                body = new Color(0.78f, 0.78f, 0.86f, 1f);
                shade = new Color(0.32f, 0.36f, 0.48f, 1f);
                accent = new Color(0.42f, 0.72f, 1f, 1f);
                break;
            case EnemyKind.SparkSpitter:
                body = new Color(1f, 0.74f, 0.2f, 1f);
                shade = new Color(0.54f, 0.25f, 0.08f, 1f);
                accent = new Color(1f, 0.95f, 0.45f, 1f);
                break;
            case EnemyKind.BombSprite:
                body = new Color(0.35f, 0.32f, 0.38f, 1f);
                shade = new Color(0.1f, 0.09f, 0.13f, 1f);
                accent = new Color(1f, 0.42f, 0.12f, 1f);
                eye = new Color(1f, 0.72f, 0.22f, 1f);
                break;
            case EnemyKind.FrostWisp:
                body = new Color(0.58f, 0.92f, 1f, 1f);
                shade = new Color(0.18f, 0.38f, 0.68f, 1f);
                accent = new Color(0.9f, 1f, 1f, 1f);
                break;
            case EnemyKind.DashImp:
                body = new Color(1f, 0.36f, 0.36f, 1f);
                shade = new Color(0.48f, 0.08f, 0.12f, 1f);
                accent = new Color(1f, 0.86f, 0.24f, 1f);
                break;
            case EnemyKind.HealerFairy:
                body = new Color(0.68f, 1f, 0.72f, 1f);
                shade = new Color(0.22f, 0.52f, 0.32f, 1f);
                accent = new Color(1f, 0.95f, 0.58f, 1f);
                break;
            case EnemyKind.SummonerShade:
                body = new Color(0.46f, 0.32f, 0.72f, 1f);
                shade = new Color(0.12f, 0.08f, 0.24f, 1f);
                accent = new Color(0.92f, 0.64f, 1f, 1f);
                eye = new Color(0.9f, 0.72f, 1f, 1f);
                break;
            case EnemyKind.CrystalBrute:
                body = new Color(0.5f, 0.9f, 1f, 1f);
                shade = new Color(0.12f, 0.28f, 0.48f, 1f);
                accent = new Color(0.94f, 1f, 1f, 1f);
                break;
            default:
                body = new Color(0.56f, 1f, 0.48f, 1f);
                shade = new Color(0.2f, 0.55f, 0.24f, 1f);
                accent = new Color(0.86f, 1f, 0.54f, 1f);
                break;
        }
    }

    static bool Ellipse(float u, float v, float cx, float cy, float rx, float ry)
    {
        float x = (u - cx) / rx;
        float y = (v - cy) / ry;
        return x * x + y * y < 1f;
    }

    static bool RoundedBox(float u, float v, float cx, float cy, float hx, float hy)
    {
        return Mathf.Abs(u - cx) < hx && Mathf.Abs(v - cy) < hy;
    }

    static bool EyePair(float u, float v, float ax, float ay, float bx, float by, float r)
    {
        return Ellipse(u, v, ax, ay, r, r * 1.2f) || Ellipse(u, v, bx, by, r, r * 1.2f);
    }

    static bool Star(float u, float v, float cx, float cy, float radius)
    {
        float angle = Mathf.Atan2(v - cy, u - cx);
        float distance = Vector2.Distance(new Vector2(u, v), new Vector2(cx, cy));
        float edge = radius * (0.72f + 0.28f * Mathf.Sin(angle * 5f));
        return distance < edge;
    }

    static bool TriangleRight(float u, float v, float cx, float cy, float width, float height)
    {
        float x = (u - cx) / width;
        float y = Mathf.Abs(v - cy) / height;
        return x >= -0.5f && x <= 0.5f && y <= x + 0.5f;
    }

    static bool TriangleLeft(float u, float v, float cx, float cy, float width, float height)
    {
        return TriangleRight(1f - u, v, 1f - cx, cy, width, height);
    }

    static bool TriangleUp(float u, float v, float cx, float cy, float width, float height)
    {
        float y = (v - cy) / height;
        float x = Mathf.Abs(u - cx) / width;
        return y >= -0.5f && y <= 0.5f && x <= y + 0.5f;
    }

    static bool TriangleDown(float u, float v, float cx, float cy, float width, float height)
    {
        float y = (cy - v) / height;
        float x = Mathf.Abs(u - cx) / width;
        return y >= -0.5f && y <= 0.5f && x <= y + 0.5f;
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
