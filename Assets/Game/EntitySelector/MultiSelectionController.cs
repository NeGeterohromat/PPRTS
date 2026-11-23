using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiSelectionController
{
    [SerializeField] private RectTransform _selectionBox;

    private Canvas _canvas;
    private RectTransform _canvasRect;
    private Camera _mainCamera;

    [DI] private SelectionManager _selection;
    [DI] private UnityUpdateAspect _updateAspect;

    private bool _isSelecting;
    private Vector2 _startPosition;
    private Vector2 _endPosition;

    public bool IsSelecting => _isSelecting;

    public void Init(RectTransform box)
    {
        _mainCamera = Camera.main;
        _selectionBox = box;
        _canvas = box.GetComponentInParent<Canvas>();
        _canvasRect = _canvas.transform as RectTransform;
    }

    public void Begin(Vector2 startScreenPos)
    {
        _startPosition = startScreenPos;
        _endPosition = startScreenPos;
        _isSelecting = true;
        if (_selectionBox != null) _selectionBox.gameObject.SetActive(true);
    }

    public void UpdateDrag(Vector2 currentScreenPos)
    {
        _endPosition = currentScreenPos;
        UpdateSelectionBox();
    }

    public void Finish(Vector2 endScreenPos)
    {
        _endPosition = endScreenPos;
        UpdateSelectionBox();
        ApplySelection();
        _isSelecting = false;
        if (_selectionBox != null) _selectionBox.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        _isSelecting = false;
        if (_selectionBox != null) _selectionBox.gameObject.SetActive(false);
    }

    private void UpdateSelectionBox()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, _startPosition, null, out Vector2 localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, _endPosition, null, out Vector2 localEnd);

        Vector2 min = Vector2.Min(localStart, localEnd);
        Vector2 max = Vector2.Max(localStart, localEnd);

        _selectionBox.anchoredPosition = (min + max) * 0.5f;
        _selectionBox.sizeDelta = max - min;
    }

    private void ApplySelection()
    {
        _selection.Clear();

        Vector2 min = Vector2.Min(_startPosition, _endPosition);
        Vector2 max = Vector2.Max(_startPosition, _endPosition);

        foreach (var entity in _updateAspect.VisibleEntityIterator)
        {
            ref var transformComponent = ref _updateAspect.TransformDataPool.Get(entity);
            Vector3 screenPosition = _mainCamera.WorldToScreenPoint(transformComponent.Position);
            if (screenPosition.z <= 0) continue;

            Vector2 position = new(screenPosition.x, screenPosition.y);
            if (position.x >= min.x && position.x <= max.x &&
                position.y >= min.y && position.y <= max.y)
            {
                _selection.AddHighlight(entity);
            }
        }

        _selection.IsLastClickInEmpty = false;
    }
}