using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;

public class ToggleRotatingCommand : ICommand
{
    private int entityID;
    public ToggleRotatingCommand(int entityId)
    {
        entityID = entityId;
    }

    public void Execute(Dictionary<int, ProtoPackedEntityWithWorld> dict)
    {
        dict[entityID].TryUnpack(out var world, out var entity);
        var aspect = (RotationAspect)world.Aspect(typeof(RotationAspect));
        if (aspect.CubeRotationPool.Has(entity))
        {
            ref var rotationComponent = ref aspect.CubeRotationPool.Get(entity);
            rotationComponent.isRotating = !rotationComponent.isRotating;
        }
    }
}
