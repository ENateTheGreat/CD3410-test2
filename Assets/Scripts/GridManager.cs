/* Author: E. Nathan Lee
 * Date: 12/13/2025
 * Description: Grid manager that handles the basis for the game functionality.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // Grid dimensions
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    public Transform plane; // Reference plane

    public Vector3 origin = Vector3.zero; // Grid origin

    public static GridManager Instance; // Self

    private void Awake()
    {
        if (Instance != null && Instance != this) // If grid exists, destroy duplicate
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Vector3 GridToWorld(Vector2Int gridPosition) // Convert grid to world position
    {
        float y = plane != null ? plane.position.y : origin.y; // Set grid y based on plane
        return origin + new Vector3(gridPosition.x * cellSize, y + 0.5f, gridPosition.y * cellSize); // Return world position (with tuning to center grid on plane)
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition) // Convert world position to grid
    {
        Vector3 local = worldPosition - origin; // Get local position
        int x = Mathf.RoundToInt(local.x / cellSize); // Calc X
        int y = Mathf.RoundToInt(local.z / cellSize); // Calc Y
        return new Vector2Int(x, y); // Return grid position
    }

    public bool IsInsideGrid(Vector2Int gridPosition) // Snake death check (outside of grid)
    {
        return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
    }

}
