using UnityEngine;

/// Legacy classroom shooter spawner kept disabled for the final dungeon project.
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

    void Update()
    {
        // Dungeon Key Run places guards in GameBootstrap instead of spawning waves.
    }
}
