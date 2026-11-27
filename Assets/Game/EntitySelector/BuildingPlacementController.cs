using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacementController : IProtoInitService
{
    [Header("Настройки сетки")]
    public float GridSize = 1.0f;
    public LayerMask GroundLayer;

    private GameObject _prefabToBuild;
    private GameObject _ghostObject;

    [DI] private ICommandAdder _adder;
    [DI] private ThisPlayerID _playerID;
    [DI] private GameAspect aspect;
    [DI] private SelectionManager selectionManager;
    private Camera _mainCamera; // или брать Camera.main внутри

    public bool IsPlacing => _prefabToBuild != null && _ghostObject != null;

    public void Init(IProtoSystems systems)
    {
        _mainCamera = Camera.main;
    }

    public void StartPlacement(GameObject prefab)
    {
        _prefabToBuild = prefab;

        if (_ghostObject != null) Object.Destroy(_ghostObject);

        _ghostObject = Object.Instantiate(_prefabToBuild);

        foreach (var col in _ghostObject.GetComponentsInChildren<Collider>())
            Object.Destroy(col);

        var authoring = _ghostObject.GetComponent<ResourceGeneratorAuthoring>();
        if (authoring != null) Object.Destroy(authoring);

        SetGhostColor(_ghostObject, new Color(1f, 1f, 1f, 0.5f));
    }

    public void HandlePlacementInput(Ray ray, Camera camera)
    {
        if (_prefabToBuild == null || _ghostObject == null) return;

        UpdateGhostPosition(ray, camera);

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryPlaceBuilding(ray);
        else if (Mouse.current.rightButton.wasPressedThisFrame)
            Cancel();
    }

    public void Cancel()
    {
        _prefabToBuild = null;
        if (_ghostObject != null) Object.Destroy(_ghostObject);
        _ghostObject = null;
    }

    private void UpdateGhostPosition(Ray ray, Camera camera)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GroundLayer))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.9f)
            {
                _ghostObject.SetActive(false);
                return;
            }
            _ghostObject.SetActive(true);

            int x = Mathf.FloorToInt(hit.point.x / GridSize);
            int z = Mathf.FloorToInt(hit.point.z / GridSize);
            Vector2Int gridPos = new Vector2Int(x, z);

            var gridData = _prefabToBuild.GetComponent<BuildingGridData>();
            int sizeX = gridData != null ? gridData.GridSize.x : 1;
            int sizeZ = gridData != null ? gridData.GridSize.y : 1;

            float offsetX = (sizeX * GridSize) / 2f;
            float offsetZ = (sizeZ * GridSize) / 2f;
            Vector3 ghostPos = new Vector3(x * GridSize + offsetX, hit.point.y, z * GridSize + offsetZ);

            _ghostObject.transform.position = ghostPos;

            if (BuildingGridManager.Instance.IsAreaFree(gridPos, sizeX, sizeZ))
                SetGhostColor(_ghostObject, new Color(0f, 1f, 0f, 0.5f));
            else
                SetGhostColor(_ghostObject, new Color(1f, 0f, 0f, 0.5f));
        }
        else
        {
            _ghostObject.SetActive(false);
        }
    }

    private void TryPlaceBuilding(Ray ray)
    {
        if (_adder == null && EcsStartup.InstanceQueue != null) _adder = EcsStartup.InstanceQueue;
        if (_adder == null) return;

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, GroundLayer)) return;
        if (Vector3.Dot(hit.normal, Vector3.up) < 0.9f) return;

        int x = Mathf.FloorToInt(hit.point.x / GridSize);
        int z = Mathf.FloorToInt(hit.point.z / GridSize);
        Vector2Int gridPos = new Vector2Int(x, z);

        var gridData = _prefabToBuild.GetComponent<BuildingGridData>();
        int sizeX = gridData != null ? gridData.GridSize.x : 1;
        int sizeZ = gridData != null ? gridData.GridSize.y : 1;

        if (!BuildingGridManager.Instance.IsAreaFree(gridPos, sizeX, sizeZ)) return;

        if (selectionManager.Highlights.Count == 0) { Debug.Log("0 builders are selected"); Cancel(); return; }

        if (!TryPlayerPayPrice()) { Cancel(); return; }

        _adder.Add(new PlaceBuildingCommand(gridPos, _prefabToBuild, hit.point.y, _playerID.ID, selectionManager.Highlights.ToArray()));

        selectionManager.Build(hit.point);

        Debug.Log($"hit {hit.point}");

        BuildingGridManager.Instance.OccupyArea(gridPos, sizeX, sizeZ);

        Cancel();
    }

    private bool TryPlayerPayPrice()
    {
        var component = (PlacableBuildingComponent)_prefabToBuild.GetComponent<ProtoUnityAuthoring>()
            .Components
            .First(c => c.GetType() == typeof(PlacableBuildingComponent));

        foreach (var player in aspect.PlayerIterator)
        {
            ref var p = ref aspect.Players.Get(player);
            if (p.ID != _playerID.ID) continue;

            ref var inv = ref aspect.Inventories.Get(player);
            var canPay = inv.Money >= component.Price;
            Debug.Log($"Current money: {inv.Money}, current price: {component.Price}, {canPay}");
            if (canPay)
                inv.Money -= component.Price;
            return canPay;
        }
        return false;
    }

    private void SetGhostColor(GameObject obj, Color color)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                mat.color = color;
            }
        }
    }
}