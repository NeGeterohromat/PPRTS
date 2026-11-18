using System.Collections.Generic;
using UnityEngine;
using static BuildingData;

public class BuildingGridManager : MonoBehaviour
{
    public static BuildingGridManager Instance { get; private set; }

    public Dictionary<Vector2Int, PositionData> positions = new();

    [Header("������ ����� �������� (� �������)")]
    public int Padding = 1;
    public Vector2Int LeftBottomWorldBorder;
    public Vector2Int RightTopWorldBorder;

    private HashSet<Vector2Int> _occupiedCells = new HashSet<Vector2Int>();



    private void Awake()
    {
        Instance = this;
        SetBorders();
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
        AddPositionData(bottomLeftCell, sizeX, sizeZ);
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                _occupiedCells.Add(new Vector2Int(bottomLeftCell.x + x, bottomLeftCell.y + z));
            }
        }
    }

    public PositionData GetDataInPosition(Vector2Int position)
    {
        // Ваша реализация
        if (positions.ContainsKey(position))
        {
            return positions[position];
        }
        return new PositionData() { isBuilding = true };
    }

    private void SetBorders()
    {
        for (int x = LeftBottomWorldBorder.x; x <= RightTopWorldBorder.x; x++)
        {
            for (int y = LeftBottomWorldBorder.y; y <= RightTopWorldBorder.y; y++)
            {
                positions[new Vector2Int(x, y)] = new PositionData(); // Empty Cells
            }
        }

        for (int x = LeftBottomWorldBorder.x; x <= RightTopWorldBorder.x; x++)
        {
            positions[new Vector2Int(x, LeftBottomWorldBorder.y - 1)] = new PositionData()
            {
                isBuilding = true,
                isUpBorder = true // Bottom World Border == Empty Cell In Up
            };
            positions[new Vector2Int(x, RightTopWorldBorder.y + 1)] = new PositionData()
            {
                isBuilding = true,
                isDownBorder = true // Top World Border == Empty Cell In Down
            };
        }
        for (int y = LeftBottomWorldBorder.y; y <= RightTopWorldBorder.y; y++)
        {
            positions[new Vector2Int(LeftBottomWorldBorder.x - 1, y)] = new PositionData()
            {
                isBuilding = true,
                isRightBorder = true // Left World Border == Empty Cell In Right
            };
            positions[new Vector2Int(RightTopWorldBorder.x + 1, y)] = new PositionData()
            {
                isBuilding = true,
                isLeftBorder = true // Right World Border == Empty Cell In Left
            };
        }
    }

    private void AddPositionData(Vector2Int bottomLeftCell, int sizeX, int sizeZ)
    {
        if (sizeX == 1 && sizeZ == 1)
        {
            positions[bottomLeftCell] = new PositionData()
            {
                isBuilding = true,
                isUpBorder = true,
                isDownBorder = true,
                isLeftBorder = true,
                isRightBorder = true
            };
            return;
        }

        if (sizeX == 1)
        {
            positions[new Vector2Int(0, sizeZ - 1) + bottomLeftCell] = new PositionData()
            {
                isBuilding = true,
                isUpBorder = true,
                isLeftBorder = true,
                isRightBorder = true
            };
            positions[bottomLeftCell] = new PositionData()
            {
                isBuilding = true,
                isDownBorder = true,
                isLeftBorder = true,
                isRightBorder = true
            };
            for (int y = LeftBottomWorldBorder.y+1; y <= RightTopWorldBorder.y - 1; y++)
            {
                var pos = new Vector2Int(0, y) + bottomLeftCell;
                positions[pos] = new PositionData()
                {
                    isBuilding = true,
                    isLeftBorder = true,
                    isRightBorder = true
                };
            }
            return;
        }

        if (sizeZ == 1)
        {
            positions[new Vector2Int(sizeX - 1, 0) + bottomLeftCell] = new PositionData()
            {
                isBuilding = true,
                isUpBorder = true,
                isDownBorder = true,
                isRightBorder = true
            };
            positions[bottomLeftCell] = new PositionData()
            {
                isBuilding = true,
                isDownBorder = true,
                isUpBorder = true,
                isLeftBorder = true
            };
            for (int x = LeftBottomWorldBorder.x + 1; x <= RightTopWorldBorder.x - 1; x++)
            {
                var pos = new Vector2Int(x, 0) + bottomLeftCell;
                positions[pos] = new PositionData()
                {
                    isBuilding = true,
                    isUpBorder = true,
                    isDownBorder = true
                };
            }
            return;
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeZ; y++)
            {
                var pos = new Vector2Int(x,y) + bottomLeftCell;
                if (x == 0)
                {
                    if (y == 0) // LeftDown Edge
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isLeftBorder = true,
                            isDownBorder = true
                        };
                    } 
                    else if (y==sizeZ-1) // LeftUp Edge
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isLeftBorder = true,
                            isUpBorder = true
                        };
                    }
                    else // Side
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isLeftBorder = true
                        };
                    }
                }
                else if (x == sizeX - 1)
                {
                    if (y == 0) // RightDown Edge
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isRightBorder = true,
                            isDownBorder = true
                        };
                    }
                    else if (y == sizeZ - 1) // RightUp Edge
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isRightBorder = true,
                            isUpBorder = true
                        };
                    }
                    else // Side
                    {
                        positions[pos] = new PositionData()
                        {
                            isBuilding = true,
                            isRightBorder = true
                        };
                    }
                }
                else if (y == 0) // Edges are written before, only down side
                {
                    positions[pos] = new PositionData()
                    {
                        isBuilding = true,
                        isDownBorder = true
                    };
                }
                else if (y == sizeZ-1) // Edges are written before, only up side
                {
                    positions[pos] = new PositionData()
                    {
                        isBuilding = true,
                        isUpBorder = true
                    };
                }
                else // Inside Cells
                {
                    positions[pos] = new PositionData()
                    {
                        isBuilding = true
                    };
                }
            }
        }
    }
}

public struct PositionData
{
    public bool isBuilding;
    public bool isUpBorder;
    public bool isDownBorder;
    public bool isLeftBorder;
    public bool isRightBorder;
}