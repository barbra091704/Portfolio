using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace Core
{
    public struct GridCell
    {
        public bool isSolid;
        public bool hasBuild;
        public Transform placedBuild;
    }

    public class BuildingGrid : NetworkBehaviour, IInteractable
    {
        public static BuildingGrid Instance;

        public int gridSize = 10;
        public float cubeSize = 1.0f;
        public RemovalPattern removalPattern = RemovalPattern.None;
        public int roomWidth = 3;
        public int roomHeight = 3;
        public int maxRoomDepth = 3;
        public GameObject[] TopDecals;
        public GameObject[] BottomDecals;
        public GameObject[] SideDecals;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        public GridCell[,,] grid;


        List<Vector3> removedCubePositions = new();
        public enum RemovalPattern
        {
            None,
            Room
        }

        void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 

            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();

            if (meshFilter == null || meshRenderer == null || meshCollider == null)
            {
                Debug.LogError("MeshFilter, MeshRenderer, and MeshCollider components are required.");
                return;
            }

            grid = new GridCell[gridSize, gridSize, gridSize];

            InitializeGrid();
            ApplyRemovalPattern();
            GenerateMesh();
            PlaceDecalsOnRemovedPositions();
        }

        void InitializeGrid()
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        grid[x, y, z] = new GridCell
                        {
                            isSolid = true,
                            hasBuild = false
                        };
                    }
                }
            }
        }

        void ApplyRemovalPattern()
        {
            switch (removalPattern)
            {
                case RemovalPattern.Room:
                    CreateRandomRoom();
                    break;
            }
        }

        void CreateRandomRoom()
        {
            // Define the center of the grid
            int centerX = gridSize / 2;
            int centerZ = gridSize / 2;

            // Ensure room dimensions are within grid bounds
            int halfWidth = roomWidth / 2;
            int halfHeight = roomHeight / 2;

            int startX = Mathf.Clamp(centerX - halfWidth, 0, gridSize - roomWidth);
            int startZ = Mathf.Clamp(centerZ - halfHeight, 0, gridSize - roomHeight);

            int endX = Mathf.Clamp(centerX + halfWidth, 0, gridSize);
            int endZ = Mathf.Clamp(centerZ + halfHeight, 0, gridSize);

            // Randomly choose the depth of the room within the max depth allowed
            int roomDepth = Random.Range(1, maxRoomDepth + 1);

            // Mark cubes within the room as removed and collect their positions
            for (int x = startX; x < endX; x++)
            {
                for (int z = startZ; z < endZ; z++)
                {
                    for (int y = centerX - roomDepth / 2; y <= centerX + roomDepth / 2; y++)
                    {
                        if (y >= 0 && y < gridSize)
                        {
                            grid[x, y, z].isSolid = false;
                            grid[x, y, z].hasBuild = true;

                            removedCubePositions.Add(GetGridCenter(new(x,y,z)));
                        }
                    }
                }
            }
        }

        void PlaceDecalsOnRemovedPositions()
        {
            foreach (Vector3 position in removedCubePositions)
            {
                PlaceDecals(position);
            }
        }

        void GenerateMesh()
        {
            List<Vector3> vertices = new();
            List<int> triangles = new();

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        if (grid[x, y, z].isSolid)
                        {
                            AddCube(vertices, triangles, x, y, z);
                        }
                    }
                }
            }

            Mesh mesh = new()
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        void AddCube(List<Vector3> vertices, List<int> triangles, int x, int y, int z)
        {
            // Calculate position relative to the center of the grid
            float halfSize = gridSize * cubeSize / 2f;
            Vector3 p0 = new((x * cubeSize) - halfSize, (y * cubeSize) - halfSize, (z * cubeSize) - halfSize);
            Vector3 p1 = new((x + 1) * cubeSize - halfSize, (y * cubeSize) - halfSize, (z * cubeSize) - halfSize);
            Vector3 p2 = new((x + 1) * cubeSize - halfSize, (y + 1) * cubeSize - halfSize, (z * cubeSize) - halfSize);
            Vector3 p3 = new((x * cubeSize) - halfSize, (y + 1) * cubeSize - halfSize, (z * cubeSize) - halfSize);
            Vector3 p4 = new((x * cubeSize) - halfSize, (y * cubeSize) - halfSize, (z + 1) * cubeSize - halfSize);
            Vector3 p5 = new((x + 1) * cubeSize - halfSize, (y * cubeSize) - halfSize, (z + 1) * cubeSize - halfSize);
            Vector3 p6 = new((x + 1) * cubeSize - halfSize, (y + 1) * cubeSize - halfSize, (z + 1) * cubeSize - halfSize);
            Vector3 p7 = new((x * cubeSize) - halfSize, (y + 1) * cubeSize - halfSize, (z + 1) * cubeSize - halfSize);

            // Front
            if (z == 0 || !grid[x, y, z - 1].isSolid)
            {
                AddQuad(vertices, triangles, p0, p1, p2, p3);
            }
            // Back
            if (z == gridSize - 1 || !grid[x, y, z + 1].isSolid)
            {
                AddQuad(vertices, triangles, p5, p4, p7, p6);
            }
            // Left
            if (x == 0 || !grid[x - 1, y, z].isSolid)
            {
                AddQuad(vertices, triangles, p4, p0, p3, p7);
            }
            // Right
            if (x == gridSize - 1 || !grid[x + 1, y, z].isSolid)
            {
                AddQuad(vertices, triangles, p1, p5, p6, p2);
            }
            // Bottom
            if (y == 0 || !grid[x, y - 1, z].isSolid)
            {
                AddQuad(vertices, triangles, p4, p5, p1, p0);
            }
            // Top
            if (y == gridSize - 1 || !grid[x, y + 1, z].isSolid)
            {
                AddQuad(vertices, triangles, p3, p2, p6, p7);
            }
        }

        void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl)
        {
            int startIndex = vertices.Count;
            vertices.Add(bl);
            vertices.Add(br);
            vertices.Add(tr);
            vertices.Add(tl);

            // Add triangles for the quad
            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 2);
        }

        public void Interact<T>(RaycastHit hit, NetworkObject player, T type = default)
        {
            Vector3 localPoint = transform.InverseTransformPoint(hit.point);
            Vector3Int gridCoords = new(
                Mathf.FloorToInt((localPoint.x + (gridSize * cubeSize / 2f)) / cubeSize),
                Mathf.FloorToInt((localPoint.y + (gridSize * cubeSize / 2f)) / cubeSize),
                Mathf.FloorToInt((localPoint.z + (gridSize * cubeSize / 2f)) / cubeSize)
            );

            // Adjust gridCoords based on hit normal to ensure correct grid cell is targeted
            Vector3 hitNormal = hit.normal;
            if (Mathf.Abs(hitNormal.x) > Mathf.Abs(hitNormal.y) && Mathf.Abs(hitNormal.x) > Mathf.Abs(hitNormal.z))
            {
                gridCoords.x += (hitNormal.x > 0) ? -1 : 0;
            }
            else if (Mathf.Abs(hitNormal.y) > Mathf.Abs(hitNormal.x) && Mathf.Abs(hitNormal.y) > Mathf.Abs(hitNormal.z))
            {
                gridCoords.y += (hitNormal.y > 0) ? -1 : 0;
            }
            else
            {
                gridCoords.z += (hitNormal.z > 0) ? -1 : 0;
            }

            // Ensure gridCoords are within bounds
            gridCoords.x = Mathf.Clamp(gridCoords.x, 0, gridSize - 1);
            gridCoords.y = Mathf.Clamp(gridCoords.y, 0, gridSize - 1);
            gridCoords.z = Mathf.Clamp(gridCoords.z, 0, gridSize - 1);

            
            // Prevent removal if it's an edge cube or if a build is present
            if (gridCoords.x == 0 || gridCoords.x == gridSize - 1 ||
                gridCoords.y == 0 || gridCoords.y == gridSize - 1 ||
                gridCoords.z == 0 || gridCoords.z == gridSize - 1)
            {
                Debug.Log("You Reached The Border.");
                return;
            }

            if (grid[gridCoords.x, gridCoords.y + 1, gridCoords.z].hasBuild)
            {
                Debug.Log("Cannot remove supporting cube with a build on it.");
                return;
            }

            // Check 3x3x3 area around the cube for any grid cell with hasBuild set to true
            bool hasBuildNearby = false;
            for (int x = gridCoords.x - 1; x <= gridCoords.x + 1; x++)
            {
                for (int y = gridCoords.y - 1; y <= gridCoords.y + 1; y++)
                {
                    for (int z = gridCoords.z - 1; z <= gridCoords.z + 1; z++)
                    {
                        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize && z >= 0 && z < gridSize)
                        {
                            if (grid[x, y, z].hasBuild)
                            {
                                hasBuildNearby = true;
                                break;
                            }
                        }
                    }
                    if (hasBuildNearby) break;
                }
                if (hasBuildNearby) break;
            }

            if (!hasBuildNearby)
            {
                Debug.Log("Cannot remove cube, no builds within 3x3.");
                return;
            }

            // Only allow removal if not an edge cube, is currently solid, and has no build and has support
            if (grid[gridCoords.x, gridCoords.y, gridCoords.z].isSolid)
            {
                grid[gridCoords.x, gridCoords.y, gridCoords.z].isSolid = false;

                GenerateMesh();

                PlaceDecals(GetGridCenter(gridCoords));
            }
        }

        private void PlaceDecals(Vector3 worldCubeCenter)
        {
            // Directions for checking each face
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            GameObject[][] decals = { TopDecals, BottomDecals, SideDecals, SideDecals, SideDecals, SideDecals };
            Quaternion[] rotations = {
                Quaternion.identity, // Top
                Quaternion.identity,
                Quaternion.Euler(0, -90, 0), // Left
                Quaternion.Euler(0, 90, 0), // Right
                Quaternion.Euler(0, 0, 0), // Forward
                Quaternion.Euler(0, 180, 0) // Back
            };

            // Loop through each direction
            for (int i = 0; i < directions.Length; i++)
            {
                if (Physics.Raycast(worldCubeCenter, directions[i], out RaycastHit hit, 5f))
                {
                    if (hit.transform.CompareTag("Grid"))
                    {
                        Instantiate(decals[i][Random.Range(0, decals[i].Length)], worldCubeCenter, rotations[i], transform);
                    }
                    else if (hit.transform.CompareTag("Decal"))
                    {
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }

        public Vector3 GetGridCenter(Vector3 gridCoords)
        {
            Vector3 cubeCenter = new(
                (gridCoords.x + 0.5f) * cubeSize - (gridSize * cubeSize / 2f),
                (gridCoords.y + 0.5f) * cubeSize - (gridSize * cubeSize / 2f),
                (gridCoords.z + 0.5f) * cubeSize - (gridSize * cubeSize / 2f)
            );
            return transform.TransformPoint(cubeCenter);
        }
    }
}