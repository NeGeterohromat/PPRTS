using UnityEngine;

public class PlaceBuildingCommand : ICommand
{
    private readonly Vector2Int _gridPosition;
    private readonly GameObject _buildingPrefab;
    private readonly float _yPosition; // <-- НОВОЕ ПОЛЕ

    // Добавляем аргумент yPosition в конструктор
    public PlaceBuildingCommand(Vector2Int gridPosition, GameObject buildingPrefab, float yPosition)
    {
        _gridPosition = gridPosition;
        _buildingPrefab = buildingPrefab;
        _yPosition = yPosition;
    }

    public void Execute()
    {
        float cellSize = 1.0f;

        // Теперь используем реальную высоту, а не 0f
        Vector3 worldPosition = new Vector3(_gridPosition.x * cellSize, _yPosition, _gridPosition.y * cellSize);

        GameObject newBuilding = Object.Instantiate(_buildingPrefab, worldPosition, Quaternion.identity);
        newBuilding.name = $"Building_{_gridPosition.x}_{_gridPosition.y}";

        Debug.Log($"Построено здание на клетке {_gridPosition}. Высота: {_yPosition}");
    }
}