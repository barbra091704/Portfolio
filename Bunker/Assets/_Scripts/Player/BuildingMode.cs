using UnityEngine;
using Core;
using System;

public class BuildingMode : MonoBehaviour
{
    private BuildingGrid buildingGrid;
    private InputManager inputManager;
    private int currentPartIndex = 0;
    private bool isActive;
    private float currentRotation;

    public LayerMask layerMask;
    public GameObject[] buildParts;
    public GameObject[] buildPreviews;
    private GameObject selectedBuildingPart;
    private GameObject selectedBuildingPartPreview;
    private MeshRenderer selectedBuildingPreviewMeshRenderer;

    public Material valid;
    public Material invalid;

    public new Camera camera;

    void Start()
    {
        inputManager = InputManager.Instance;
        buildingGrid = BuildingGrid.Instance;
        Debug.Log("BuildingMode initialized.");
    }

    void Update()
    {
        if (inputManager.TabbedThisFrame())
        {
            if (isActive && selectedBuildingPartPreview != null)
            {
                Destroy(selectedBuildingPartPreview);
            }
            else
            {
                SelectBuildingPart(currentPartIndex);
            }

            isActive = !isActive;

            Debug.Log($"Building Mode is {isActive}");
        }

        if (isActive)
        {
            float scrollInput = inputManager.GetScrollWheel().normalized.y;

            if (scrollInput != 0)
            {
                currentRotation += (int)scrollInput * 90f;
                currentRotation %= 360f; 
                Debug.Log($"Current Rotation: {currentRotation}");
            }

            if (inputManager.RKeyPressed())
            {
                currentPartIndex = (currentPartIndex + 1) % buildParts.Length;

                SelectBuildingPart(currentPartIndex);

                Debug.Log($"Selected Building Part: {selectedBuildingPart.name}");
            }
            
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hitInfo, 10f, layerMask))
            { 
                if (hitInfo.transform.CompareTag("Grid") || hitInfo.transform.CompareTag("Build"))
                {
                    Vector3 localPoint = buildingGrid.transform.InverseTransformPoint(hitInfo.point);
                    Vector3Int gridCoords = new(
                        Mathf.FloorToInt((localPoint.x + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize),
                        Mathf.FloorToInt((localPoint.y + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize),
                        Mathf.FloorToInt((localPoint.z + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize)
                    );

                    // Ensure gridCoords are within bounds
                    gridCoords.x = Mathf.Clamp(gridCoords.x, 0, buildingGrid.gridSize - 1);
                    gridCoords.y = Mathf.Clamp(gridCoords.y, 0, buildingGrid.gridSize - 1);
                    gridCoords.z = Mathf.Clamp(gridCoords.z, 0, buildingGrid.gridSize - 1);


                    Debug.Log($"Calculated grid coordinates: {gridCoords}");


                        // Check if any neighboring grid cells have a build
                        bool canPlaceBuild = false;
                        Vector3Int[] neighbors = new Vector3Int[]
                        {
                            new(gridCoords.x + 1, gridCoords.y, gridCoords.z),
                            new(gridCoords.x - 1, gridCoords.y, gridCoords.z),
                            new(gridCoords.x + 1, gridCoords.y + 1, gridCoords.z),
                            new(gridCoords.x - 1, gridCoords.y + 1, gridCoords.z),
                            new(gridCoords.x + 1, gridCoords.y - 1, gridCoords.z),
                            new(gridCoords.x - 1, gridCoords.y - 1, gridCoords.z),
                            new(gridCoords.x, gridCoords.y, gridCoords.z + 1),
                            new(gridCoords.x, gridCoords.y, gridCoords.z - 1),
                            new(gridCoords.x, gridCoords.y + 1, gridCoords.z + 1),
                            new(gridCoords.x, gridCoords.y + 1, gridCoords.z - 1),
                            new(gridCoords.x, gridCoords.y - 1, gridCoords.z + 1),
                            new(gridCoords.x, gridCoords.y - 1, gridCoords.z - 1),
                        };

                        foreach (var neighbor in neighbors)
                        {
                            if (neighbor.x >= 0 && neighbor.x < buildingGrid.gridSize &&
                                neighbor.y >= 0 && neighbor.y < buildingGrid.gridSize &&
                                neighbor.z >= 0 && neighbor.z < buildingGrid.gridSize)
                            {
                                if (buildingGrid.grid[neighbor.x, neighbor.y, neighbor.z].hasBuild)
                                {
                                    canPlaceBuild = true;
                                    break;
                                }
                            }
                        }

                    Vector3 buildPosition = buildingGrid.GetGridCenter(gridCoords);

                    if (selectedBuildingPartPreview != null && !buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].hasBuild && !buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].isSolid)
                    {
                        selectedBuildingPartPreview.transform.SetPositionAndRotation(buildPosition, Quaternion.Euler(0, currentRotation, 0));

                        Material[] materials = selectedBuildingPreviewMeshRenderer.materials;

                        for (int i = 0; i < materials.Length; i++)
                        {
                            materials[i] = canPlaceBuild ? valid : invalid;

                            selectedBuildingPreviewMeshRenderer.materials = materials;

                            print("Changed material to: " + materials[i].name);
                        }
                    }

                    if (inputManager.LeftClickPressed())
                    {
                        // Check if the cube at gridCoords is not solid and doesn't already have a build
                        if (canPlaceBuild && !buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].isSolid && !buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].hasBuild)
                        {
                            PlaceBuild(gridCoords, buildPosition, new(0,currentRotation,0));

                            buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].hasBuild = true;
                        }
                        else
                        {
                            Debug.Log("Cannot place build: No buildable neighbor or cell already occupied/solid.");
                        }
                    }
                }
                if (inputManager.RightClickPressed())
                {
                    if (hitInfo.transform.CompareTag("Grid") || hitInfo.transform.CompareTag("Build"))
                    {
                        Vector3 localPoint = buildingGrid.transform.InverseTransformPoint(hitInfo.point);
                        Vector3Int gridCoords = new(
                            Mathf.FloorToInt((localPoint.x + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize),
                            Mathf.FloorToInt((localPoint.y + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize),
                            Mathf.FloorToInt((localPoint.z + (buildingGrid.gridSize * buildingGrid.cubeSize / 2f)) / buildingGrid.cubeSize)
                        );

                        // Ensure gridCoords are within bounds
                        gridCoords.x = Mathf.Clamp(gridCoords.x, 0, buildingGrid.gridSize - 1);
                        gridCoords.y = Mathf.Clamp(gridCoords.y, 0, buildingGrid.gridSize - 1);
                        gridCoords.z = Mathf.Clamp(gridCoords.z, 0, buildingGrid.gridSize - 1);


                        Debug.Log($"Calculated grid coordinates: {gridCoords}");

                        if (buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].hasBuild)
                        {
                            Destroy(buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].placedBuild.gameObject);
                            buildingGrid.grid[gridCoords.x, gridCoords.y, gridCoords.z].hasBuild = false;
                        }
                    }
                }
            }
        }
    }

    private void SelectBuildingPart(int currentPartIndex)
    {
        selectedBuildingPart = buildParts[currentPartIndex];

        if (selectedBuildingPartPreview != null) 
            Destroy(selectedBuildingPartPreview);

        selectedBuildingPartPreview = Instantiate(buildPreviews[currentPartIndex].gameObject, transform.position, Quaternion.identity);
        selectedBuildingPreviewMeshRenderer = selectedBuildingPartPreview.GetComponent<MeshRenderer>();
    }

    private void PlaceBuild(Vector3Int gridCoords, Vector3 position, Vector3 rotation)
    {
        Transform placedBuild = Instantiate(selectedBuildingPart, position, Quaternion.Euler(rotation)).transform;
    
        buildingGrid.grid[gridCoords.x,gridCoords.y,gridCoords.z].placedBuild = placedBuild;

        Debug.Log($"Placed Build at: {position}");
    }
}
