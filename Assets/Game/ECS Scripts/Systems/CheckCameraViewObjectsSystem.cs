using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class CheckCameraViewObjectsSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private UnityUpdateAspect unityAspect;
    [DI] private CameraMovementAspect cameraAspect;
    [DI] private ProtoIt CameraIt = new(new[] 
    {
        typeof(CameraTagComponent),
        typeof(TransformDataComponent)
    });

    public void Init(IProtoSystems systems)
    {
    }

    public void Run()
    {
        foreach (var camera in CameraIt)
        {
            ref var cameraTransform = ref unityAspect.TransformDataPool.Get(camera);
            foreach (var entity in unityAspect.TransformDataIterator)
            {
                ref var transformComponent = ref unityAspect.TransformDataPool.Get(entity);
                if (IsObjectVisible(cameraTransform.Position,transformComponent.Position))
                {
                    if (!unityAspect.NeedTransformUpdateByCameraEventPool.Has(entity))
                        unityAspect.NeedTransformUpdateByCameraEventPool.Add(entity);
                }
            }
        }
    }

    private bool IsObjectVisible(Vector3 cameraPos, Vector3 objPos)
    {
        return (cameraPos - objPos).magnitude < 50;
    }
}
