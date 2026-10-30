using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSelector : MonoBehaviour, IProtoInitSystem
{
    private Camera mainCamera;
    private Dictionary<ProtoPackedEntityWithWorld, HighlightableObject> highlights = new();
    [DI] private InputSystem_Actions actions;
    [DI] ICommandAdder adder;

    [Header("Настройки строительства")]
    public GameObject BuildingPrefab;
    public float GridSize = 1.0f;
    public LayerMask GroundLayer;

    private bool _isBuildMode = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void Init(IProtoSystems systems)
    {
        if (actions != null)
            actions.Player.ToggleRotate.performed += OnToggleRotate;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        // Включение/выключение режима стройки (ПКМ)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _isBuildMode = !_isBuildMode;
            DeselectCurrent();
            Debug.Log($"Режим строительства: {(_isBuildMode ? "ВКЛ" : "ВЫКЛ")}");
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (_isBuildMode)
            {
                TryPlaceBuilding(ray);
            }
            else
            {
                TrySelectObject(ray);
            }
        }
    }

    private void TryPlaceBuilding(Ray ray)
    {
        if (BuildingPrefab == null) { /* ... */ return; }

        // Резервный путь, если DI не сработал
        if (adder == null && EcsStartup.InstanceQueue != null)
        {
            adder = EcsStartup.InstanceQueue;
        }

        if (adder == null)
        {
            Debug.LogError("adder равен null и InstanceQueue тоже null!");
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GroundLayer))
        {
            int x = Mathf.RoundToInt(hit.point.x / GridSize);
            int z = Mathf.RoundToInt(hit.point.z / GridSize);
            Vector2Int gridPos = new Vector2Int(x, z);

            // Передаем hit.point.y третьим аргументом!
            adder.Add(new PlaceBuildingCommand(gridPos, BuildingPrefab, hit.point.y));

            _isBuildMode = false;
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
            }
            else
            {
                DeselectCurrent();
            }
        }
        else
        {
            DeselectCurrent();
        }
    }

    private void SelectObject(HighlightableObject h, ProtoUnityAuthoring p)
    {
        h.Highlight();
        highlights[p.Entity()] = h;
    }

    private void DeselectCurrent()
    {
        foreach (var h in highlights.Values)
            h.Unhighlight();
        highlights.Clear();
    }

    private void OnToggleRotate(InputAction.CallbackContext context)
    {
        if (adder == null) return;

        foreach (var e in highlights.Keys)
        {
            adder.Add(new ToggleRotatingCommand(e));
        }
    }
}