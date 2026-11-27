using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;

public class MovementAspect : ProtoAspectInject
{
    public ProtoPool<HasMovementGoalComponent> MovementGoalPool;
    public ProtoPool<NeedCalculatePathEventComponent> NeedCalculatePathPool;
    public ProtoIt InMovementIterator = new(new[]
        {
            typeof(HasMovementGoalComponent),
            typeof(TransformDataComponent),
            typeof(HasCommandTagComponent)
        });
    public ProtoIt NeedCalculatePathIterator = new(new[]
    {
        typeof(NeedCalculatePathEventComponent),
        typeof(HasMovementGoalComponent),
        typeof(TransformDataComponent)
    });
}

public struct NeedCalculatePathEventComponent { }

public struct HasMovementGoalComponent
{
    public Vector2 GoalPosition;          // Исходная цель (может быть внутри постройки)
    public Vector2 ResolvedGoalPosition;  // Ближайшая свободная точка снаружи
    public bool IsPathCalculated;
    public List<Vector2> PathPoints;      // Точки пути в мировых координатах (y=0)
    public int CurrentPathIndex;
}