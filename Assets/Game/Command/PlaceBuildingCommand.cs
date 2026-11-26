using Leopotam.EcsProto.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class PlaceBuildingCommand : ICommand
{
    private readonly Vector2Int _gridPosition;
    private readonly GameObject _buildingPrefab;
    private readonly float _yPosition;
    private readonly int _ownerID;

    public PlaceBuildingCommand(Vector2Int gridPosition, GameObject buildingPrefab, float yPosition, int ownerID)
    {
        _gridPosition = gridPosition;
        _buildingPrefab = buildingPrefab;
        _yPosition = yPosition;
        _ownerID = ownerID;
    }

    public void Execute()
    {
        float cellSize = 1.0f;

        // ����� ������ ������ �� �������, ��� � � MouseSelector
        BuildingGridData gridData = _buildingPrefab.GetComponent<BuildingGridData>();
        int sizeX = gridData != null ? gridData.GridSize.x : 1;
        int sizeZ = gridData != null ? gridData.GridSize.y : 1;

        // ���������� ������ (����� ��� ��, ��� �������)
        float offsetX = (sizeX * cellSize) / 2f;
        float offsetZ = (sizeZ * cellSize) / 2f;

        Vector3 worldPosition = new Vector3(
            _gridPosition.x * cellSize + offsetX,
            _yPosition,
            _gridPosition.y * cellSize + offsetZ
        );

        var obj = Object.Instantiate(_buildingPrefab, worldPosition, Quaternion.identity);

        var comps = obj.GetComponent<ProtoUnityAuthoring>().Components;
        var ownerComp = comps.Where(c => c.GetType() == typeof(OwnerComponent)).First();
        comps.Remove(ownerComp);

        var owner = new OwnerComponent() { OwnerID = _ownerID };

        comps.Add(owner);

        Debug.Log($"��������� ������ �� {_gridPosition}");
    }
}