using UnityEngine;
using Leopotam.EcsProto;
using System;

public class CubeRotationModule : IProtoModule
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
        systems.AddSystem(new CubeRotationSystem());
    }
}
