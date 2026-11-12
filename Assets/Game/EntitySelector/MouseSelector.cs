using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MouseSelector : MonoBehaviour, IProtoInitSystem
{
    private Camera mainCamera;
    [DI] private InputSystem_Actions actions;
    [DI] ICommandAdder adder;

    private Dictionary<ProtoPackedEntityWithWorld, HighlightableObject> highlights = new();

    private GameObject _prefabToBuild = null;
    private GameObject _ghostObject = null; 

    [Header("Настройки сетки")]
    public float GridSize = 1.0f;
    public LayerMask GroundLayer;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void Init(IProtoSystems systems)
    {
        if (actions != null) actions.Player.ToggleRotate.performed += OnToggleRotate;
    }

    public void StartPlacementMode(GameObject prefab)
    {
        _prefabToBuild = prefab;

        if (_ghostObject != null) Destroy(_ghostObject); 

        _ghostObject = Instantiate(_prefabToBuild);

        foreach (var col in _ghostObject.GetComponentsInChildren<Collider>()) Destroy(col);

        var authoring = _ghostObject.GetComponent<ResourceGeneratorAuthoring>();
        if (authoring != null) Destroy(authoring);

        SetGhostColor(_ghostObject, new Color(1f, 1f, 1f, 0.5f));
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (_prefabToBuild != null && _ghostObject != null)
        {
            UpdateGhostPosition(ray);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPlaceBuilding(ray);
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelBuilding();
            }
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySelectObject(ray);
        }
    }

    private void UpdateGhostPosition(Ray ray)
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

            BuildingGridData gridData = _prefabToBuild.GetComponent<BuildingGridData>();
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
        if (adder == null && EcsStartup.InstanceQueue != null) adder = EcsStartup.InstanceQueue;
        if (adder == null) return;

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GroundLayer))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.9f)
            {
                return;
            }

            int x = Mathf.FloorToInt(hit.point.x / GridSize);
            int z = Mathf.FloorToInt(hit.point.z / GridSize);
            Vector2Int gridPos = new Vector2Int(x, z);

            BuildingGridData gridData = _prefabToBuild.GetComponent<BuildingGridData>();
            int sizeX = gridData != null ? gridData.GridSize.x : 1;
            int sizeZ = gridData != null ? gridData.GridSize.y : 1;

            if (!BuildingGridManager.Instance.IsAreaFree(gridPos, sizeX, sizeZ))
            {
                return;
            }

            adder.Add(new PlaceBuildingCommand(gridPos, _prefabToBuild, hit.point.y));
            BuildingGridManager.Instance.OccupyArea(gridPos, sizeX, sizeZ);

            CancelBuilding();
        }
    }

    private void CancelBuilding()
    {
        _prefabToBuild = null;
        if (_ghostObject != null) Destroy(_ghostObject);
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

    private void TrySelectObject(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Component component = hit.collider;
            HighlightableObject highlightComp = component.GetComponent<HighlightableObject>();
            ProtoUnityAuthoring authoringComp = component.GetComponent<ProtoUnityAuthoring>();

            if (highlightComp != null && authoringComp != null)
            {
                SelectObject(highlightComp, authoringComp);
                ResourceGeneratorAuthoring resGen = component.GetComponent<ResourceGeneratorAuthoring>();
                if (resGen != null && BuildingUIController.Instance != null)
                {
                    string title = resGen.ResourceToProduce == ResourceType.Wood ? "Лесопилка" :
                                   resGen.ResourceToProduce == ResourceType.Coal ? "Угольная шахта" : "Постройка";
                    BuildingUIController.Instance.ShowDetails(title, $"Добыча: {resGen.ResourceToProduce}", $"{resGen.BaseRate} / {resGen.CycleTime} сек");
                }
            }
            else
            {
                DeselectCurrent();
                if (BuildingUIController.Instance != null) BuildingUIController.Instance.DetailsPanel.SetActive(false);
            }
        }
        else
        {
            DeselectCurrent();
            if (BuildingUIController.Instance != null) BuildingUIController.Instance.DetailsPanel.SetActive(false);
        }
    }

    private void SelectObject(HighlightableObject h, ProtoUnityAuthoring p)
    {
        h.Highlight();
        highlights[p.Entity()] = h;
    }

    private void DeselectCurrent()
    {
        foreach (var h in highlights.Values) h.Unhighlight();
        highlights.Clear();
    }

    private void OnToggleRotate(InputAction.CallbackContext context)
    {
        if (adder == null) return;
        foreach (var e in highlights.Keys) adder.Add(new ToggleRotatingCommand(e));
    }
}