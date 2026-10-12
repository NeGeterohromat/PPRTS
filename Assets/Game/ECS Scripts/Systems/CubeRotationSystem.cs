using Leopotam.EcsProto;
using UnityEngine;

public sealed class CubeRotationSystem : IProtoInitSystem, IProtoRunSystem
{
    private RotationAspect _rotationAspect;
    private UnityUpdateAspect _unityAspect;

    private ProtoIt CubeRotationIterator;

    public void Init(IProtoSystems systems)
    {
        ProtoWorld world = systems.World();

        _rotationAspect = (RotationAspect)world.Aspect(typeof(RotationAspect));
        _unityAspect = (UnityUpdateAspect)world.Aspect(typeof(UnityUpdateAspect));

        CubeRotationIterator = new ProtoIt(
            new[]
            {
                typeof(CubeRotationComponent),
                typeof(TransformDataComponent)
            }
        );

        CubeRotationIterator.Init(world);
    }

    public void Run()
    {
        foreach (ProtoEntity entity in CubeRotationIterator)
        {
            ref CubeRotationComponent rotation =
                ref _rotationAspect.CubeRotationPool.Get(entity);
            ref var transform = ref _unityAspect.TransformDataPool.Get(entity);

            if (rotation.isRotating)
            {
                transform.Rotation.eulerAngles += new Vector3(0,0.1f,0);
                if (!_unityAspect.NeedTransformUpdateEventPool.Has(entity))
                    _unityAspect.NeedTransformUpdateEventPool.Add(entity);
            }
        }
    }
}