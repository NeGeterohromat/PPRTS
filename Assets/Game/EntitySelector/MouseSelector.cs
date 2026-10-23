using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// 1. Подключаем пространство имен нового Input System
using UnityEngine.InputSystem;

public class MouseSelector : MonoBehaviour
{
    private Camera mainCamera;
    private Dictionary<ProtoPackedEntityWithWorld,HighlightableObject> highlights = new();
    [DI] private InputSystem_Actions actions;
    [DI] ICommandAdder adder;

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
    }

    void Update()
    {
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
                        SelectObject(highlightComp, authoringComp);
                    }
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
        foreach (var e in highlights.Keys)
        {
            adder.Add(new ToggleRotatingCommand(e));
        }
    }
}