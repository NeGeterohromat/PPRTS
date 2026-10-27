using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;
using System;

public class MovementModule : IProtoModule
{
    public void Init(IProtoSystems systems)
    {
        systems
            .AddSystem(new PathCalculationSystem())
            .AddSystem(new MovementSystem());
    }

    public IProtoAspect[] Aspects() => null; // аспект регистрируется в мире отдельно
    public Type[] Dependencies() => null;
}