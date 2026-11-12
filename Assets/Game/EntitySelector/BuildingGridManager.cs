using System.Collections.Generic;
using UnityEngine;

public class BuildingGridManager : MonoBehaviour
{
    public static BuildingGridManager Instance { get; private set; }

    [Header("Разрыв между зданиями (в клетках)")]
    public int Padding = 1; 

    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();

    private void Awake()
    {
        Instance = this;
    }

    public bool IsAreaFree(Vector2Int bottomLeftCell, int sizeX, int sizeZ)
    {
        for (int x = -Padding; x < sizeX + Padding; x++)
        {
            for (int z = -Padding; z < sizeZ + Padding; z++)
            {
                if (_occupiedCells.Contains(new Vector2Int(bottomLeftCell.x + x, bottomLeftCell.y + z)))
                    return false;
            }
        }
        return true;
    }

    public void OccupyArea(Vector2Int bottomLeftCell, int sizeX, int sizeZ)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                _occupiedCells.Add(new Vector2Int(bottomLeftCell.x + x, bottomLeftCell.y + z));
            }
        }
    }
}