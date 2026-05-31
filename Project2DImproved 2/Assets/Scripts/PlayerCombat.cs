using UnityEngine;

/// Mouse-aimed combat controller for the top-down player.
public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting")]
    public float fireCooldown = 0.22f;
    public float bulletSpeed = 12f;
    public int damage = 1;
    public float bulletLifetime = 1.2f;
    public int extraProjectiles;
    public float spreadAngle = 24f;
    public float minFireCooldown = 0.08f;
    public float rapidFireMultiplier = 0.82f;
    public int damagePerUpgrade = 1;

    [Header("Setup")]
    public GameObject bulletPrefab;
    public float muzzleOffset = 0.48f;
    public AudioClip shootClip;

    Camera mainCamera;
    AudioSource audioSrc;
    float cooldownTimer;
    float baseFireCooldown;
    int baseDamage;
    Vector2 aimDirection = Vector2.up;

    void Awake()
    {
        mainCamera = Camera.main;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    void Start()
    {
        baseFireCooldown = fireCooldown;
        baseDamage = damage;
        if (GameManager.I != null) GameManager.I.OnGameStarted += ResetForNewRun;
        ResetForNewRun();
    }

    void OnDestroy()
    {
        if (GameManager.I != null) GameManager.I.OnGameStarted -= ResetForNewRun;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        AimAtMouse();

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (Input.GetMouseButton(0)) TryFire();
    }

    void AimAtMouse()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        Vector2 toMouse = mouseWorld - transform.position;
        if (toMouse.sqrMagnitude < 0.0001f) return;

        aimDirection = toMouse.normalized;
        transform.up = aimDirection;
    }

    void TryFire()
    {
        if (cooldownTimer > 0f) return;

        cooldownTimer = fireCooldown;
        SpawnBulletVolley();

        if (shootClip != null) audioSrc.PlayOneShot(shootClip, 0.45f);
    }

    public void ApplyUpgrade(WeaponUpgradeKind upgrade)
    {
        switch (upgrade)
        {
            case WeaponUpgradeKind.ExtraProjectile:
                extraProjectiles = Mathf.Min(4, extraProjectiles + 2);
                break;
            case WeaponUpgradeKind.RapidFire:
                fireCooldown = Mathf.Max(minFireCooldown, fireCooldown * rapidFireMultiplier);
                cooldownTimer = Mathf.Min(cooldownTimer, fireCooldown);
                break;
            case WeaponUpgradeKind.DamageUp:
                damage += Mathf.Max(1, damagePerUpgrade);
                break;
        }
    }

    void ResetForNewRun()
    {
        fireCooldown = baseFireCooldown;
        damage = baseDamage;
        extraProjectiles = 0;
        cooldownTimer = 0f;
    }

    void SpawnBulletVolley()
    {
        int projectileCount = Mathf.Clamp(1 + extraProjectiles, 1, 5);
        if (projectileCount == 1)
        {
            SpawnBullet(aimDirection);
            return;
        }

        float totalSpread = spreadAngle * (projectileCount - 1);
        float startAngle = -totalSpread * 0.5f;
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + spreadAngle * i;
            Vector2 shotDirection = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (Vector3)aimDirection);
            SpawnBullet(shotDirection.normalized);
        }
    }

    void SpawnBullet(Vector2 direction)
    {
        Vector3 spawnPosition = transform.position + (Vector3)(direction * muzzleOffset);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        GameObject bullet = bulletPrefab != null
            ? Instantiate(bulletPrefab, spawnPosition, rotation)
            : BuildFallbackBullet(spawnPosition, rotation);

        bullet.SetActive(true);

        Bullet projectile = bullet.GetComponent<Bullet>();
        if (projectile != null)
        {
            projectile.speed = bulletSpeed;
            projectile.damage = damage;
            projectile.lifetime = bulletLifetime;
        }
    }

    GameObject BuildFallbackBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = new GameObject("Player Bullet");
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.transform.localScale = Vector3.one * 0.18f;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = Art2D.SolidCircle(new Color(1f, 0.82f, 0.25f), 32);
        sr.sortingOrder = 4;

        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        bullet.AddComponent<Bullet>();
        return bullet;
    }
}
