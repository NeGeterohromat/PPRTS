using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class MovementSystem : IProtoRunSystem
{
    [DI] private MovementAspect _aspect;
    [DI] private UnityUpdateAspect _updateAspect;
    private float _speed = 5f; // настройте под вашу игру

    public void Run()
    {
        foreach (var entity in _aspect.InMovementIterator)
        {
            ref var goal = ref _aspect.MovementGoalPool.Get(entity);
            ref var transform = ref _updateAspect.TransformDataPool.Get(entity);

            if (!goal.IsPathCalculated || goal.PathPoints == null || goal.PathPoints.Count == 0)
                continue;

            if (goal.CurrentPathIndex >= goal.PathPoints.Count)
            {
                // Дошли до конца
                continue;
            }

            Vector3 currentPos = transform.Position;
            Vector2 target = goal.PathPoints[goal.CurrentPathIndex];
            Vector3 target3D = new Vector3(target.x, 0, target.y);

            Vector3 toTarget = target3D - currentPos;
            float distance = toTarget.magnitude;
            float step = _speed * Time.deltaTime;

            bool moved = false;
            bool rotated = false;

            if (distance <= step)
            {
                transform.Position = target3D;
                goal.CurrentPathIndex++;
                moved = true;
            }
            else if (distance > 0.001f)
            {
                Vector3 dir = toTarget / distance;
                transform.Position += dir * step;
                moved = true;

                // Плавный поворот
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                if (Quaternion.Angle(transform.Rotation, targetRot) > 0.5f)
                {
                    transform.Rotation = Quaternion.RotateTowards(transform.Rotation, targetRot, 360f * Time.deltaTime);
                    rotated = true;
                }
            }

            if (moved || rotated)
            {
                // Добавляем событие только если его ещё нет
                if (!_updateAspect.NeedTransformUpdateEventPool.Has(entity))
                {
                    _updateAspect.NeedTransformUpdateEventPool.Add(entity);
                }
            }
        }
    }
}
