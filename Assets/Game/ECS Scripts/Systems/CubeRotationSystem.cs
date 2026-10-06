using Leopotam.EcsProto;
using UnityEngine;

public sealed class CubeRotationSystem : IProtoInitSystem, IProtoRunSystem
{
    private RotationAspect _aspect;

    public void Init(IProtoSystems systems)
    {
        ProtoWorld world = systems.World();

        _aspect = (RotationAspect)world.Aspect(typeof(RotationAspect));
    }

    public void Run()
    {
        foreach (ProtoEntity entity in _aspect.CubeRotationIterator)
        {
            ref CubeRotationComponent visibility =
                ref _aspect.CubeRotationPool.Get(entity);

            if (visibility.View == null)
            {
                continue;
            }

            if (visibility.isRotating)
            {
                visibility.View.transform.Rotate(new Vector3(1,0,0));
            }
        }
    }
}