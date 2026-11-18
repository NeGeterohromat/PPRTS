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
        systems.AddSystem(new CommandExecuteSystem())
            .AddSystem(new CheckCameraViewObjectsSystem())
            .AddSystem(new UnityUpdateTransformSystem())
            .AddSystem(new HighlightSystem());
    }
}
