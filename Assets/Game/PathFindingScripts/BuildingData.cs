using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData
{
    private Dictionary<Vector2Int, PositionData> positions = new()
{
    // Строка 0 (y=0)
    {new Vector2Int(0, 0), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = true,
        isLeftBorder = true,
        isRightBorder = false
    }},
    {new Vector2Int(1, 0), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(2, 0), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(3, 0), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(4, 0), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = true
    }},
    
    // Строка 1 (y=1)
    {new Vector2Int(0, 1), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    {new Vector2Int(1, 1), new PositionData()
    {
        isBuilding = false,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(2, 1), new PositionData()
    {
        isBuilding = false,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(3, 1), new PositionData()
    {
        isBuilding = false,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(4, 1), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    
    // Строка 2 (y=2)
    {new Vector2Int(0, 2), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    {new Vector2Int(1, 2), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = true,
        isRightBorder = false
    }},
    {new Vector2Int(2, 2), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = true
    }},
    {new Vector2Int(3, 2), new PositionData()
    {
        isBuilding = false,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(4, 2), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    
    // Строка 3 (y=3)
    {new Vector2Int(0, 3), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    {new Vector2Int(1, 3), new PositionData()
    {
        isBuilding = false,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(2, 3), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = true,
        isRightBorder = false
    }},
    {new Vector2Int(3, 3), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = true
    }},
    {new Vector2Int(4, 3), new PositionData()
    {
        isBuilding = true,
        isUpBorder = false,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = true
    }},
    
    // Строка 4 (y=4)
    {new Vector2Int(0, 4), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = false,
        isLeftBorder = true,
        isRightBorder = false
    }},
    {new Vector2Int(1, 4), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(2, 4), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(3, 4), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = true,
        isLeftBorder = false,
        isRightBorder = false
    }},
    {new Vector2Int(4, 4), new PositionData()
    {
        isBuilding = true,
        isUpBorder = true,
        isDownBorder = false,
        isLeftBorder = false,
        isRightBorder = true
    }}
};

    public struct PositionData
    {
        public bool isBuilding;
        public bool isUpBorder;
        public bool isDownBorder;
        public bool isLeftBorder;
        public bool isRightBorder;
    }

    public PositionData GetDataInPosition(Vector2Int position)
    {
        // Ваша реализация
        if (positions.ContainsKey(position)) 
        {
            var p = positions[position];
            Debug.Log(position);
            Debug.Log($"{p.isBuilding} {p.isUpBorder} {p.isDownBorder} {p.isLeftBorder} {p.isRightBorder}");
            return positions[position]; 
        }
        Debug.Log("sss");
        Debug.Log(position);
        return new PositionData() { isBuilding = false };
    }
}
