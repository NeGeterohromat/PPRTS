using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public class ObjectSelectionController
{
    private GameObject _lastClickedVisual;

    [DI] private SelectionManager _selection;
    [DI] private GameAspect _gameAspect;
    [DI] private ThisPlayerID _playerID;

    public void SetLastClickedVisual(GameObject visual)
    {
        _lastClickedVisual = visual;
    }

    public void HandleClick(Ray ray)
    {
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            _selection.Clear();
            if (BuildingUIController.Instance != null)
                BuildingUIController.Instance.DetailsPanel.SetActive(false);
            return;
        }

        Component component = hit.collider;
        var highlightComp = component.GetComponent<HighlightableObject>();
        var authoringComp = component.GetComponent<ProtoUnityAuthoring>();

        if (highlightComp != null)
        {
            if (authoringComp != null)
            {
                SelectFromObject(authoringComp);

                var resGen = component.GetComponent<ResourceGeneratorAuthoring>();
                if (resGen != null && BuildingUIController.Instance != null)
                {
                    string title = resGen.ResourceToProduce == ResourceType.Wood ? "Лесопилка" :
                                   resGen.ResourceToProduce == ResourceType.Coal ? "Угольная шахта" :
                                   "Постройка";
                    BuildingUIController.Instance.ShowDetails(
                        title,
                        $"Добыча: {resGen.ResourceToProduce}",
                        $"{resGen.BaseRate} / {resGen.CycleTime} сек");
                }
            }
        }
        else
        {
            if (_selection.IsLastClickInEmpty)
            {
                _selection.Clear();
                _selection.LastClickedPoint = Vector3.zero;
                _selection.IsLastClickInEmpty = false;
                if (_lastClickedVisual != null) _lastClickedVisual.SetActive(false);
            }
            else
            {
                _selection.LastClickedPoint = hit.point;
                _selection.IsLastClickInEmpty = true;
                if (_lastClickedVisual != null)
                {
                    _lastClickedVisual.transform.position = hit.point;
                    _lastClickedVisual.SetActive(true);
                }
            }
        }
    }

    private void SelectFromObject(ProtoUnityAuthoring authoring)
    {
        authoring.Entity().TryUnpack(out _, out var e);

        if (!_gameAspect.OwnerComponents.Has(e) ||
            _gameAspect.OwnerComponents.Get(e).OwnerID != _playerID.ID) return;

        _selection.AddHighlight(authoring.Entity());
    }
}