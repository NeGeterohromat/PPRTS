using Leopotam.EcsProto;
using UnityEngine;
using System;

public class CameraMovementModule : IProtoModule
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
        systems.AddSystem(new CameraMovementSystem());
    }
}
