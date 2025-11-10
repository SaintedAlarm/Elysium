using UnityEngine;
using Unity.AI.Navigation;

public class SeamlessWorldTerrainManager : MonoBehaviour
{
        [Header("Navigation")]
    public NavMeshSurface navMeshSurface;

    [Header("Tile Settings")]
    [Tooltip("Your Terrain tile prefab (must have a SeamlessTerrainChunk on it).")]
    public Terrain tilePrefab;

    [Tooltip("Number of tiles along X (east-west).")]
    public int tilesX = 3;

    [Tooltip("Number of tiles along Z (north-south).")]
    public int tilesZ = 3;

    [Tooltip("Size of each tile in world units. Must match how you want the world spaced.")]
    public float tileSize = 512f;

    [Header("Terrain Resolution")]
    [Tooltip("Heightmap resolution for each tile (power of two + 1, e.g., 257, 513).")]
    public int heightmapResolution = 257;

    [Header("Noise Settings (Global)")]
    [Tooltip("Smaller = smoother terrain, larger = more detail.")]
    public float noiseScale = 0.003f;

    [Tooltip("Max terrain height in world units.")]
    public float heightScale = 80f;

    [Tooltip("Seed so the same world can be regenerated.")]
    public int worldSeed = 12345;

        [Header("Spawning (Player & Enemies)")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;

    [Tooltip("How many enemies to place in the world.")]
    public int enemyCount = 10;

    [Header("World Decoration")]
    [Tooltip("Placeable tree prefabs (will pick randomly).")]
    public GameObject[] treePrefabs;

    [Tooltip("Placeable rock prefabs (will pick randomly).")]
    public GameObject[] rockPrefabs;

    [Tooltip("Placeable bush prefabs (will pick randomly).")]
    public GameObject[] bushPrefabs;

    [Tooltip("How many decorations of each type to scatter in the starter area.")]
    public int treeCount = 40;
    public int rockCount = 25;
    public int bushCount = 30;

    [Tooltip("Margin from world edges to avoid spawning stuff right on the border.")]
    public float spawnMargin = 20f;


    private Terrain[,] tiles;

    void Start()
    {
        GenerateWorld();
    }

    public void GenerateWorld()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("SeamlessWorldTerrainManager: tilePrefab is not assigned.");
            return;
        }

        // Clear any previously generated tiles
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        tiles = new Terrain[tilesX, tilesZ];

        // Base TerrainData from prefab, used only as a template
        TerrainData baseData = tilePrefab.terrainData;

        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, z * tileSize);

                Terrain newTile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                newTile.name = $"Terrain_Tile_{x}_{z}";

                TerrainData td = Instantiate(baseData);
                td.heightmapResolution = heightmapResolution;
                td.size = new Vector3(tileSize, heightScale, tileSize);
                newTile.terrainData = td;

                tiles[x, z] = newTile;

                SeamlessTerrainChunk chunk = newTile.GetComponent<SeamlessTerrainChunk>();
                if (chunk != null)
                {
                    Vector2 worldOffset = new Vector2(x * tileSize, z * tileSize);
                    chunk.GenerateHeights(worldOffset, tileSize, noiseScale, heightScale, worldSeed);
                }
            }
        }

        SetupNeighbors();

        // Build NavMesh AFTER terrain tiles are created
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }

        // After terrain & navmesh are ready, spawn entities and decorations
        SpawnPlayer();
        SpawnTrees();
        SpawnRocks();
        SpawnBushes();
        SpawnEnemies();


    }

    private void SetupNeighbors()
    {
        // Unity's order is: left, top, right, bottom
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                Terrain left   = (x > 0)          ? tiles[x - 1, z] : null;
                Terrain right  = (x < tilesX - 1) ? tiles[x + 1, z] : null;
                Terrain bottom = (z > 0)          ? tiles[x, z - 1] : null;
                Terrain top    = (z < tilesZ - 1) ? tiles[x, z + 1] : null;

                tiles[x, z].SetNeighbors(left, top, right, bottom);
            }
        }
    }

    // ---------- NEW HELPERS BELOW THIS LINE ----------

    private float GetHeightAt(Vector3 worldPos)
    {
        int tx = Mathf.FloorToInt(worldPos.x / tileSize);
        int tz = Mathf.FloorToInt(worldPos.z / tileSize);

        if (tx < 0 || tx >= tilesX || tz < 0 || tz >= tilesZ)
            return 0f;

        Terrain t = tiles[tx, tz];
        if (t == null) return 0f;

        float y = t.SampleHeight(worldPos) + t.transform.position.y;
        return y;
    }

    private void SpawnPlayer()
{
    if (playerPrefab == null) return;

    float worldSizeX = tilesX * tileSize;
    float worldSizeZ = tilesZ * tileSize;

    // Start near world center
    Vector3 pos = new Vector3(worldSizeX / 2f, 0f, worldSizeZ / 2f);
    pos.y = GetHeightAt(pos) + 1f;  // a bit above ground

    GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);

    // Hook camera to this player
    Camera cam = Camera.main;
    if (cam != null)
    {
        CameraControler camCtrl = cam.GetComponent<CameraControler>();
        if (camCtrl != null)
        {
            camCtrl.target = player.transform;
        }
    }
}

    private void SpawnTrees()
    {
        ScatterPrefabs(treePrefabs, treeCount);
    }

    private void SpawnRocks()
    {
        ScatterPrefabs(rockPrefabs, rockCount);
    }

    private void SpawnBushes()
    {
        ScatterPrefabs(bushPrefabs, bushCount);
    }

    /// <summary>
    /// Spawns a random selection of prefabs from the given array across the terrain.
    /// </summary>
    private void ScatterPrefabs(GameObject[] prefabs, int count)
    {
        if (prefabs == null || prefabs.Length == 0 || count <= 0) return;

        float worldSizeX = tilesX * tileSize;
        float worldSizeZ = tilesZ * tileSize;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) continue;

            float x = Random.Range(spawnMargin, worldSizeX - spawnMargin);
            float z = Random.Range(spawnMargin, worldSizeZ - spawnMargin);

            Vector3 pos = new Vector3(x, 0f, z);
            pos.y = GetHeightAt(pos);

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Vector3 scale = Vector3.one * Random.Range(0.8f, 1.3f);

            GameObject obj = Instantiate(prefab, pos, rot, transform);
            obj.transform.localScale = scale;
        }
    }


    private void SpawnEnemies()
    {
        if (enemyPrefab == null || enemyCount <= 0) return;

        float worldSizeX = tilesX * tileSize;
        float worldSizeZ = tilesZ * tileSize;

        for (int i = 0; i < enemyCount; i++)
        {
            float x = Random.Range(worldSizeX * 0.25f, worldSizeX * 0.75f);
            float z = Random.Range(worldSizeZ * 0.25f, worldSizeZ * 0.75f);

            Vector3 pos = new Vector3(x, 0f, z);
            pos.y = GetHeightAt(pos);

            Instantiate(enemyPrefab, pos, Quaternion.identity, transform);
        }
    }
}
