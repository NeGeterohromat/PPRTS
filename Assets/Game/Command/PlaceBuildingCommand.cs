using UnityEngine;

public class PlaceBuildingCommand : ICommand
{
    private readonly Vector2Int _gridPosition;
    private readonly GameObject _buildingPrefab;
    private readonly float _yPosition;

    public PlaceBuildingCommand(Vector2Int gridPosition, GameObject buildingPrefab, float yPosition)
    {
        _gridPosition = gridPosition;
        _buildingPrefab = buildingPrefab;
        _yPosition = yPosition;
    }

    public void Execute()
    {
        float cellSize = 1.0f;

        BuildingGridData gridData = _buildingPrefab.GetComponent<BuildingGridData>();
        int sizeX = gridData != null ? gridData.GridSize.x : 1;
        int sizeZ = gridData != null ? gridData.GridSize.y : 1;

        float offsetX = (sizeX * cellSize) / 2f;
        float offsetZ = (sizeZ * cellSize) / 2f;

        Vector3 worldPosition = new Vector3(
            _gridPosition.x * cellSize + offsetX,
            _yPosition,
            _gridPosition.y * cellSize + offsetZ
        );

        Object.Instantiate(_buildingPrefab, worldPosition, Quaternion.identity);
        Debug.Log($"Построено здание на {_gridPosition}");
    }
}