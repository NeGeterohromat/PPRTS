using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.EventSystems;

public class MouseSelector : MonoBehaviour, IProtoInitService
{
    [SerializeField] private GameObject LastClickedVisual;
    [SerializeField] private RectTransform _selectionBox;
    private Camera mainCamera;
    private HashSet<ProtoPackedEntityWithWorld> highlights = new();
    private bool isLastClickInEmpty = false;
    private Vector3 lastClickedPoint;

    [DI] private UnityUpdateAspect updateAspect;
    [DI] private GameAspect gameAspect;
    [DI] private InputSystem_Actions actions;
    [DI] private ICommandAdder adder;
    [DI] private ThisPlayerID playerID;

    private bool _isMultySelecting;
    private Vector2 _startPosition;
    private Vector2 _endPosition;

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
        actions.Player.ToggleRotate.performed += OnToggleRotate;
        actions.Player.MoveEntities.performed += OnMoveEntities;
        actions.Player.CreateCircleEntity.performed += OnCreateCircleEntity;
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

        // break selection logic
        //if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
        //return;

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
        else
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
    
    
    
        if (!actions.UI.Alt.IsPressed())
        {
            if (_isMultySelecting)
                UnviewMultySelectionBox();

            // 2. Проверяем клик через новый Input System
            // Mouse.current.leftButton.wasPressedThisFrame - это аналог Input.GetMouseButtonDown(0)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 3. Получаем актуальную позицию мыши на экране
                Vector2 mousePosition = Mouse.current.position.ReadValue();

                // Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Component component = hit.collider;
                    HighlightableObject highlightComp = component.GetComponent<HighlightableObject>();
                    ProtoUnityAuthoring authoringComp = component.GetComponent<ProtoUnityAuthoring>();

                    if (highlightComp != null)
                    {
                        if (authoringComp != null)
                        {
                            SelectFromObject(authoringComp);
                            
                            ResourceGeneratorAuthoring resGen = component.GetComponent<ResourceGeneratorAuthoring>();
                            if (resGen != null && BuildingUIController.Instance != null)
                            {
                                string title = resGen.ResourceToProduce == ResourceType.Wood ? "Лесопилка" :
                                resGen.ResourceToProduce == ResourceType.Coal ? "Угольная шахта" : "Постройка";
                                BuildingUIController.Instance.ShowDetails(title, $"Добыча: {resGen.ResourceToProduce}", $"{resGen.BaseRate} / {resGen.CycleTime} сек");
                            }
                        }
                    }
                    else
                    {
                        if (isLastClickInEmpty)
                        {
                            DeselectCurrent();
                            lastClickedPoint = Vector3.zero;
                            isLastClickInEmpty = false;
                        }
                        else
                        {
                            lastClickedPoint = hit.point;
                            isLastClickInEmpty = true;
                            LastClickedVisual.transform.position = lastClickedPoint;
                            LastClickedVisual.SetActive(true);
                        }
                    }
                }
                else
                {
                    DeselectCurrent();
                    if (BuildingUIController.Instance != null) BuildingUIController.Instance.DetailsPanel.SetActive(false);
                }
            }
        }
        else
        {
            if (actions.UI.Click.WasPressedThisFrame())
                StartMultySelection();

            if (_isMultySelecting && actions.UI.Click.IsPressed())
                UpdateMultySelection();

            if (_isMultySelecting && actions.UI.Click.WasReleasedThisFrame())
                FinishMultySelection();
        }
    }
    
    



                

           


    private void StartMultySelection()
    {
        _startPosition = Mouse.current.position.ReadValue();
        _endPosition = _startPosition;

        _isMultySelecting = true;

        if (_selectionBox != null)
            _selectionBox.gameObject.SetActive(true);
    }

    private void UpdateMultySelection()
    {
        _endPosition = Mouse.current.position.ReadValue();

        UpdateSelectionBox();
    }

    private void FinishMultySelection()
    {
        Debug.Log("sssssssssssssssssssssssss");

        _endPosition = Mouse.current.position.ReadValue();

        UpdateSelectionBox();
        MultySelectEntities();

        _isMultySelecting = false;

        if (_selectionBox != null)
            _selectionBox.gameObject.SetActive(false);
    }

    private void UnviewMultySelectionBox()
    {
        _isMultySelecting = false;

        if (_selectionBox != null)
            _selectionBox.gameObject.SetActive(false);
    }

    private void UpdateSelectionBox()
    {
        if (_selectionBox == null)
            return;

        Vector2 min = Vector2.Min(_startPosition, _endPosition);
        Vector2 max = Vector2.Max(_startPosition, _endPosition);

        _selectionBox.position = (min + max) * 0.5f;
        _selectionBox.sizeDelta = max - min;
    }

    private void MultySelectEntities()
    {
        DeselectCurrent();

        Vector2 min = Vector2.Min(_startPosition, _endPosition);
        Vector2 max = Vector2.Max(_startPosition, _endPosition);

        foreach (var entity in updateAspect.VisibleEntityIterator)
        {
            if (gameAspect.OwnerComponents.Get(entity).OwnerID != playerID.ID) continue;

            ref var transformComponent = ref updateAspect.TransformDataPool.Get(entity);

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(
                transformComponent.Position
            );

            if (screenPosition.z <= 0)
                continue;

            Vector2 position = new(
                screenPosition.x,
                screenPosition.y
            );

            bool inside =
                position.x >= min.x &&
                position.x <= max.x &&
                position.y >= min.y &&
                position.y <= max.y;

            if (!inside)
                continue;

            ref var highlightTag = ref updateAspect.HighlightedTagPool.Add(entity);
            highlightTag.IsGOHighlighted = false;

            var pack = updateAspect.World().PackEntityWithWorld(entity);
            highlights.Add(pack);
        }

        isLastClickInEmpty = false;
    }

    private void SelectFromObject(ProtoUnityAuthoring p)
    {
        p.Entity().TryUnpack(out _, out var e);

        if (!gameAspect.OwnerComponents.Has(e) ||
            gameAspect.OwnerComponents.Get(e).OwnerID != playerID.ID) return;

        if (!updateAspect.HighlightedTagPool.Has(e))
        {
            ref var highlightTag = ref updateAspect.HighlightedTagPool.Add(e);
            highlightTag.IsGOHighlighted = false;
        }
        highlights.Add(p.Entity());
    }

    private void DeselectCurrent()
    {
        foreach (var h in highlights)
        {
            h.TryUnpack(out _, out var e);
            if (!updateAspect.UnhighlightEventPool.Has(e))
                updateAspect.UnhighlightEventPool.Add(e);
        }
        highlights.Clear();
        LastClickedVisual.SetActive(false);
    }

    private void OnToggleRotate(InputAction.CallbackContext context)
    {
        foreach (var e in highlights)
        {
            adder.Add(new ToggleRotatingCommand(e));
        }
    }

    private void OnMoveEntities(InputAction.CallbackContext context)
    {
        foreach (var e in highlights)
        {
            adder.Add(new MoveEntityCommand(e, lastClickedPoint, playerID.ID));
        }
    }

    private void OnCreateCircleEntity(InputAction.CallbackContext context)
    {
        adder.Add(new CreateEntityCommand(EntityType.RotatingCircle, 
            new Vector3(lastClickedPoint.x, 0, lastClickedPoint.z), 
            playerID.ID));
    }
}