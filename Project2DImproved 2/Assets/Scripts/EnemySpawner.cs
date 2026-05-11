using UnityEngine;

/// Spawns enemies at the top of the arena. Spawn rate and types ramp up
/// with GameManager.I.level — that's the difficulty-scaling improvement.
public class EnemySpawner : MonoBehaviour
{
    public GameObject straightPrefab;
    public GameObject chaserPrefab;
    public GameObject zigzagPrefab;
    public Transform player;

    public float spawnIntervalLv1 = 1.4f;
    public float spawnIntervalMin = 0.35f;
    public float arenaHalfWidth = 8.5f;
    public float spawnY = 6.0f;

    float nextSpawn;

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        if (Time.time >= nextSpawn)
        {
            Spawn();
            nextSpawn = Time.time + CurrentInterval();
        }
    }

    float CurrentInterval()
    {
        // Level 1: 1.4s. Each level shaves ~12% until floor.
        float interval = spawnIntervalLv1 * Mathf.Pow(0.88f, GameManager.I.level - 1);
        return Mathf.Max(spawnIntervalMin, interval);
    }

    void Spawn()
    {
        int lvl = GameManager.I.level;
        // Type roll — chasers/zigzags become more common as level rises.
        float roll = Random.value;
        GameObject prefab;
        Enemy.Kind kind;
        if (roll < 0.6f - 0.05f * lvl) { prefab = straightPrefab; kind = Enemy.Kind.Straight; }
        else if (roll < 0.85f - 0.02f * lvl) { prefab = zigzagPrefab; kind = Enemy.Kind.Zigzag; }
        else { prefab = chaserPrefab; kind = Enemy.Kind.Chaser; }

        if (prefab == null) return;
        Vector3 pos = new Vector3(Random.Range(-arenaHalfWidth, arenaHalfWidth), spawnY, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var e = go.GetComponent<Enemy>();
        if (e != null)
        {
            e.kind = kind;
            e.player = player;
            // Speed scales gently per level.
            e.speed *= 1f + 0.07f * (lvl - 1);
        }
        go.SetActive(true);     // template is inactive
    }
}
