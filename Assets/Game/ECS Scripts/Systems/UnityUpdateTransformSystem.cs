using UnityEngine;
using Leopotam.EcsProto;

public class UnityUpdateTransformSystem : IProtoInitSystem, IProtoRunSystem
{
    private UnityUpdateAspect _aspect;
    public void Init(IProtoSystems systems)
    {
        ProtoWorld world = systems.World();

        _aspect = (UnityUpdateAspect)world.Aspect(typeof(UnityUpdateAspect));
    }

    public void Run()
    {
        foreach (var entity in _aspect.TransformUpdateIterator)
        {
            ref var goComponent = ref _aspect.GameObjectPool.Get(entity);
            ref var transformData = ref _aspect.TransformDataPool.Get(entity);

            goComponent.Obj.transform.position = transformData.Position;
            goComponent.Obj.transform.rotation = transformData.Rotation;

            _aspect.NeedTransformUpdateByCameraEventPool.Del(entity);
            _aspect.NeedTransformUpdateEventPool.Del(entity);
        }
    }
}
