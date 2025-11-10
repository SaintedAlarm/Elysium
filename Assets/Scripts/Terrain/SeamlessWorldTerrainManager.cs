using UnityEngine;

public class SeamlessWorldTerrainManager : MonoBehaviour
{
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
}
