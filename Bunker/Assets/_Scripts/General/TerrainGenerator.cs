using UnityEngine;
using UnityEditor;
public class TerrainGenerator : MonoBehaviour
{
    public int width = 1000;
    public int height = 1000;
    public float depth = 20f;
    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    // Additional parameters for more complex terrain
    public float hillScale = 50f;
    public float valleyScale = 10f;
    public float flatness = 0.3f; // Control the flat areas
    public float hillHeight = 5f;
    public float valleyDepth = -5f;

    public TerrainData terrainData;

    void Start()
    {
        terrainData = GetComponent<Terrain>().terrainData;
    }

    public TerrainData GenerateTerrain()
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        // Basic terrain with Perlin noise
        float baseHeight = Mathf.PerlinNoise(xCoord, yCoord);

        // Hills
        float hillCoord = (float)x / width * hillScale + offsetX;
        float hillHeightNoise = Mathf.PerlinNoise(hillCoord, yCoord) * hillHeight;

        // Valleys
        float valleyCoord = (float)x / width * valleyScale + offsetX;
        float valleyDepthNoise = Mathf.PerlinNoise(valleyCoord, yCoord) * valleyDepth;

        // Combine all heights
        float finalHeight = baseHeight + hillHeightNoise + valleyDepthNoise;

        // Apply flatness
        finalHeight = Mathf.Lerp(finalHeight, baseHeight, flatness);

        return finalHeight;
    }
}

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        DrawDefaultInspector(); // Draw the default inspector fields

        if (GUILayout.Button("Generate Terrain"))
        {
            terrainGenerator.GenerateTerrain(); // Call the GenerateTerrain method
        }
    }
}
