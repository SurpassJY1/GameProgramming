using UnityEngine;

/// Periodically drops a random power-up from the top of the arena.
public class PowerUpSpawner : MonoBehaviour
{
    public GameObject rapidPrefab;
    public GameObject shieldPrefab;
    public GameObject extraLifePrefab;

    public float intervalMin = 9f;
    public float intervalMax = 15f;
    public float arenaHalfWidth = 8.5f;
    public float spawnY = 6.0f;

    float nextSpawn;

    void Update()
    {
        if (GameManager.I == null || GameManager.I.phase != GameManager.Phase.Playing) return;

        if (Time.time >= nextSpawn)
        {
            Spawn();
            nextSpawn = Time.time + Random.Range(intervalMin, intervalMax);
        }
    }

    void Spawn()
    {
        // Weighted: Rapid common, Shield medium, ExtraLife rare.
        float r = Random.value;
        GameObject prefab =
            r < 0.5f ? rapidPrefab :
            r < 0.85f ? shieldPrefab :
            extraLifePrefab;
        if (prefab == null) return;
        Vector3 pos = new Vector3(Random.Range(-arenaHalfWidth, arenaHalfWidth), spawnY, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.SetActive(true);     // template is inactive
    }
}
