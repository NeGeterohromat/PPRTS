using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class PathCalculationSystem : IProtoRunSystem
{
    [DI] private MovementAspect _aspect;
    [DI] private UnityUpdateAspect _updateAspect;
    [DI] private PathfindingService _pathService;

    public void Run()
    {
        foreach (var entity in _aspect.NeedCalculatePathIterator)
        {
            ref var goal = ref _aspect.MovementGoalPool.Get(entity);
            ref var transform = ref _updateAspect.TransformDataPool.Get(entity);

            Vector2 start = new Vector2(transform.Position.x, transform.Position.z);
            Vector2 resolvedGoal = _pathService.ResolveGoal(goal.GoalPosition);
            goal.ResolvedGoalPosition = resolvedGoal;

            var path = _pathService.FindPath(start, resolvedGoal);
            if (path != null && path.Count > 0)
            {
                goal.PathPoints = path;
                goal.CurrentPathIndex = 0;
                goal.IsPathCalculated = true;
            }
            else
            {
                goal.IsPathCalculated = false;
                goal.PathPoints = null;
            }

            // Удаляем событие после расчёта
            _aspect.NeedCalculatePathPool.Del(entity);
        }
    }
}

