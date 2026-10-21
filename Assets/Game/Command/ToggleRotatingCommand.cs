using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;

public class ToggleRotatingCommand : ICommand
{
    private ProtoPackedEntityWithWorld entityWithWorld;
    public ToggleRotatingCommand(ProtoPackedEntityWithWorld entity)
    {
        entityWithWorld = entity;
    }

    public void Execute()
    {
        entityWithWorld.TryUnpack(out var world, out var entity);
        var aspect = (RotationAspect)world.Aspect(typeof(RotationAspect));
        if (aspect.CubeRotationPool.Has(entity))
        {
            ref var rotationComponent = ref aspect.CubeRotationPool.Get(entity);
            rotationComponent.isRotating = !rotationComponent.isRotating;
        }
    }
}
