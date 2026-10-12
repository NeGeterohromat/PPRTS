using Leopotam.EcsProto;
using UnityEngine;

public class CheckCameraViewObjectsSystem : IProtoInitSystem, IProtoRunSystem
{
    private UnityUpdateAspect _aspect;

    public void Init(IProtoSystems systems)
    {
        ProtoWorld world = systems.World();

        _aspect = (UnityUpdateAspect)world.Aspect(typeof(UnityUpdateAspect));
    }

    public void Run()
    {
        foreach (var camera in _aspect.CameraIterator)
        {
            ref var camComponent = ref _aspect.CameraPool.Get(camera);
            foreach (var entity in _aspect.TransformDataIterator)
            {
                ref var transformComponent = ref _aspect.TransformDataPool.Get(entity);
                if (IsObjectVisible(camComponent.Camera.transform.position,transformComponent.Position))
                {
                    if (!_aspect.NeedTransformUpdateByCameraEventPool.Has(entity))
                        _aspect.NeedTransformUpdateByCameraEventPool.Add(entity);
                }
            }
        }
    }

    private bool IsObjectVisible(Vector3 cameraPos, Vector3 objPos)
    {
        return (cameraPos - objPos).magnitude < 4;
    }
}
