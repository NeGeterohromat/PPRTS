using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SelectionManager
{
    private readonly HashSet<ProtoPackedEntityWithWorld> _highlights = new();

    [DI] private UnityUpdateAspect _updateAspect;
    [DI] private GameAspect _gameAspect;
    [DI] private ThisPlayerID _playerID;
    [DI] private ICommandAdder _adder;

    // Состояние "последнего клика в пустоте" — нужно для ObjectSelectionController
    public bool IsLastClickInEmpty { get; set; }
    public Vector3 LastClickedPoint { get; set; }

    public IReadOnlyCollection<ProtoPackedEntityWithWorld> Highlights => _highlights;

    public bool CanSelect(ProtoPackedEntityWithWorld packed)
    {
        if (!packed.TryUnpack(out _, out var e)) return false;
        return _gameAspect.OwnerComponents.Has(e) &&
               _gameAspect.OwnerComponents.Get(e).OwnerID == _playerID.ID;
    }

    public void AddHighlight(ProtoPackedEntityWithWorld packed)
    {
        if (!packed.TryUnpack(out _, out var e)) return;

        if (_updateAspect.HasCommandTagPool.Has(e)) return;

        if (!_updateAspect.HighlightedTagPool.Has(e))
        {
            ref var tag = ref _updateAspect.HighlightedTagPool.Add(e);
            tag.IsGOHighlighted = false;
        }
        _highlights.Add(packed);
    }

    public void AddHighlight(ProtoEntity entity)
    {
        if (!_gameAspect.OwnerComponents.Has(entity) ||
            _gameAspect.OwnerComponents.Get(entity).OwnerID != _playerID.ID) return;

        if (!_updateAspect.HighlightedTagPool.Has(entity))
        {
            ref var tag = ref _updateAspect.HighlightedTagPool.Add(entity);
            tag.IsGOHighlighted = false;
        }
        _highlights.Add(_updateAspect.World().PackEntityWithWorld(entity));
    }

    public void Clear()
    {
        foreach (var h in _highlights)
        {
            h.TryUnpack(out _, out var e);
            if (!_updateAspect.UnhighlightEventPool.Has(e))
                _updateAspect.UnhighlightEventPool.Add(e);
        }
        _highlights.Clear();
        IsLastClickInEmpty = false;
    }

    public void ToggleRotate()
    {
        foreach (var e in _highlights)
            _adder.Add(new ToggleRotatingCommand(e));
    }

    public void MoveTo(Vector3 point)
    {
        foreach (var e in _highlights)
        {
            e.TryUnpack(out var _, out var entity);
            if (!_updateAspect.HasCommandTagPool.Has(entity))
            {
                ref var comm = ref _updateAspect.HasCommandTagPool.Add(entity);
                comm.CommandType = CommandType.MoveTo;
            }

            _adder.Add(new MoveEntityCommand(e, point, _playerID.ID));
        }
    }

    public void Build(Vector3 buildingPoint)
    {
        foreach (var e in _highlights)
        {
            e.TryUnpack(out var _, out var entity);
            ref var comm = ref _updateAspect.HasCommandTagPool.Add(entity);
            comm.CommandType = CommandType.Build;

            _adder.Add(new MoveEntityCommand(e, buildingPoint, _playerID.ID));
        }
    }
}