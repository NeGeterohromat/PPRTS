using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class CameraMovementSystem : IProtoInitSystem, IProtoRunSystem
{
    private const float MoveSpeedScale = 0.01f;
    [DI] private ProtoIt CameraIterator = new(new[] 
    {
        typeof(CameraTagComponent),
        typeof(TransformDataComponent)
    });
    [DI] private InputSystem_Actions actions;
    [DI] private UnityUpdateAspect aspect;

    public void Init(IProtoSystems systems)
    {
    }

    public void Run()
    {
        var dir = actions.Player.Move.ReadValue<Vector2>();
        if (dir.Equals(Vector2.zero)) return;

        var worldDir = new Vector3(dir.x, 0, dir.y) * MoveSpeedScale;
        foreach (var entity in CameraIterator)
        {
            ref var transform = ref aspect.TransformDataPool.Get(entity);
            transform.Position += worldDir;

            if (!aspect.NeedTransformUpdateByCameraEventPool.Has(entity))
                aspect.NeedTransformUpdateByCameraEventPool.Add(entity);
            if (!aspect.NeedTransformUpdateEventPool.Has(entity))
                aspect.NeedTransformUpdateEventPool.Add(entity);
        }
    }
}