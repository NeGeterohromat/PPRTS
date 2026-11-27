using Leopotam.EcsProto.Unity;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Leopotam.EcsProto.QoL;

public class PlaceBuildingCommand : ICommand
{
    private readonly Vector2Int _gridPosition;
    private readonly GameObject _buildingPrefab;
    private readonly float _yPosition;
    private readonly int _ownerID;
    private readonly ProtoPackedEntityWithWorld[] _builders;

    public PlaceBuildingCommand(Vector2Int gridPosition, GameObject buildingPrefab, float yPosition, int ownerID, 
        params ProtoPackedEntityWithWorld[] builders)
    {
        _gridPosition = gridPosition;
        _buildingPrefab = buildingPrefab;
        _yPosition = yPosition;
        _ownerID = ownerID;
        _builders = builders;
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
        var processComp = comps.Where(c => c.GetType() == typeof(InBuildingProcessTagComponent)).First();
        var W = ((InBuildingProcessTagComponent)processComp).TimeForOneBuilder;
        comps.Remove(ownerComp);
        comps.Remove(processComp);

        var owner = new OwnerComponent() { OwnerID = _ownerID };
        var process = new InBuildingProcessTagComponent() 
        { 
            CurrentProgress = 0, 
            ProcessTime = W / (1f + 0.75f * (_builders.Length - 1)), 
            Builders = _builders,
            isBuildersCame = false
        };

        Debug.Log(string.Join(' ', _builders));

        comps.Add(owner);
        comps.Add(process);

        Debug.Log($"��������� ������ �� {_gridPosition}");
    }
}