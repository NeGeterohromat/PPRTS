using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// 1. Подключаем пространство имен нового Input System
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;

public class MouseSelector : MonoBehaviour
{
    [SerializeField] private GameObject LastClickedVisual;
    [SerializeField] private RectTransform _selectionBox;
    private Camera mainCamera;
    private HashSet<ProtoPackedEntityWithWorld> highlights = new();
    private bool isLastClickInEmpty = false;
    private Vector3 lastClickedPoint;

    [DI] private UnityUpdateAspect updateAspect;
    [DI] private InputSystem_Actions actions;
    [DI] ICommandAdder adder;

    private bool _isMultySelecting;
    private Vector2 _startPosition;
    private Vector2 _endPosition;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
    }

    public void Init(IProtoSystems systems)
    {
        actions.Player.ToggleRotate.performed += OnToggleRotate;
        actions.Player.MoveEntities.performed += OnMoveEntities;
    }

    void Update()
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

                Ray ray = mainCamera.ScreenPointToRay(mousePosition);
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
            adder.Add(new MoveEntityCommand(e, lastClickedPoint));
        }
    }
}