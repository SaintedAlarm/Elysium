using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class SeamlessTerrainChunk : MonoBehaviour
{
    /// <summary>
    /// Generate heights for this chunk using world-space offset.
    /// worldOffset is the bottom-left corner of this tile in world units.
    /// </summary>
    public void GenerateHeights(
        Vector2 worldOffset,
        float tileSize,
        float noiseScale,
        float heightScale,
        int seed)
    {
        // IMPORTANT: grab the TerrainData *now*, after the manager has assigned its clone
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        int res = terrainData.heightmapResolution;
        float[,] heights = new float[res, res];

        // Make sure the terrain size matches the tile size and height
        terrainData.size = new Vector3(tileSize, heightScale, tileSize);

        int maxIndex = res - 1;

        for (int z = 0; z < res; z++)
        {
            for (int x = 0; x < res; x++)
            {
                // Local 0–1 within this tile
                float nx = (float)x / maxIndex;
                float nz = (float)z / maxIndex;

                // Actual world-space position of this sample
                float worldX = worldOffset.x + nx * tileSize;
                float worldZ = worldOffset.y + nz * tileSize;

                float sampleX = (worldX + seed) * noiseScale;
                float sampleZ = (worldZ + seed) * noiseScale;

                float h = Mathf.PerlinNoise(sampleX, sampleZ); // 0–1

                heights[z, x] = h;
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }
}
