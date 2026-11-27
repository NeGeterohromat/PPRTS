using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MouseSelector : MonoBehaviour, IProtoInitService
{
    [SerializeField] private GameObject LastClickedVisual;
    [SerializeField] private RectTransform _selectionBox;

    [Header("Сетка / размещение")]
    public float GridSize = 1.0f;
    public LayerMask GroundLayer;

    private Camera _mainCamera;

    [DI] private SelectionManager _selection;
    [DI] private MultiSelectionController _multiSelection;
    [DI] private ObjectSelectionController _objectSelection;
    [DI] private BuildingPlacementController _buildingPlacement;
    [DI] private InputSystem_Actions _actions;
    [DI] private ICommandAdder _adder;
    [DI] private ThisPlayerID _playerID;

    private void Start()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        _objectSelection.SetLastClickedVisual(LastClickedVisual);

        // Прокидываем настройки в контроллер строительства
        _buildingPlacement.GridSize = GridSize;
        _buildingPlacement.GroundLayer = GroundLayer;

        // Прокидываем selectionBox в multi-selection
        _multiSelection.Init(_selectionBox);
    }

    public void Init(IProtoSystems systems)
    {
        _actions.Player.ToggleRotate.performed += OnToggleRotate;
        _actions.Player.MoveEntities.performed += OnMoveEntities;
        _actions.Player.CreateCircleEntity.performed += OnCreateCircleEntity;
    }

    public void StartPlacementMode(GameObject prefab)
    {
        _buildingPlacement.StartPlacement(prefab);
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (!_multiSelection.IsSelecting)
            if (EventSystem.current.IsPointerOverGameObject()) 
                return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        if (_buildingPlacement.IsPlacing)
        {
            _buildingPlacement.HandlePlacementInput(ray, _mainCamera);
            return;
        }

        if (_actions.UI.Alt.IsPressed())
        {
            HandleMultiSelectionInput(mousePosition);
        }
        else
        {
            if (_multiSelection.IsSelecting) _multiSelection.Cancel();

            if (Mouse.current.leftButton.wasPressedThisFrame)
                _objectSelection.HandleClick(ray);
        }
    }

    private void HandleMultiSelectionInput(Vector2 mousePosition)
    {
        if (_actions.UI.Click.WasPressedThisFrame())
            _multiSelection.Begin(mousePosition);

        if (_multiSelection.IsSelecting && _actions.UI.Click.IsPressed())
            _multiSelection.UpdateDrag(mousePosition);

        if (_multiSelection.IsSelecting && _actions.UI.Click.WasReleasedThisFrame())
            _multiSelection.Finish(mousePosition);
    }

    private void OnToggleRotate(InputAction.CallbackContext context) => _selection.ToggleRotate();

    private void OnMoveEntities(InputAction.CallbackContext context) => _selection.MoveTo(_selection.LastClickedPoint);

    private void OnCreateCircleEntity(InputAction.CallbackContext context)
    {
        var point = _selection.LastClickedPoint;
        _adder.Add(new CreateEntityCommand(EntityType.RotatingCircle,
            new Vector3(point.x, 0, point.z),
            _playerID.ID));
    }
}