using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab;     // GruntEnemy prefab
    public Transform[] spawnPoints;    // where to spawn from
    public int totalToSpawn = 10;      // total enemies in this round
    public float spawnInterval = 2f;   // seconds between spawns
    public bool autoStart = true;      // start spawning on Start

    private int spawnedCount = 0;
    private float nextSpawnTime = 0f;
    private bool spawning = false;

    void Start()
    {
        if (autoStart)
        {
            BeginSpawning();
        }
    }

    public void BeginSpawning()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: Missing enemyPrefab or spawnPoints.");
            return;
        }

        spawnedCount = 0;
        spawning = true;
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (!spawning) return;

        if (spawnedCount >= totalToSpawn)
        {
            // all enemies spawned, stop
            spawning = false;
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnOne();
            spawnedCount++;
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnOne()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, point.position, point.rotation);
    }
}
