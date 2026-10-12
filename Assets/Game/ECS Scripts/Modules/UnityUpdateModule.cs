using Leopotam.EcsProto;
using UnityEngine;
using System;

public class UnityUpdateModule : IProtoModule
{
    public IProtoAspect[] Aspects()
    {
        return null;
    }

    public Type[] Dependencies()
    {
        return null;
    }

    public void Init(IProtoSystems systems)
    {
        systems.AddSystem(new EntityRegistrySystem())
            .AddSystem(new CheckCameraViewObjectsSystem())
            .AddSystem(new UnityUpdateTransformSystem());
    }
}
