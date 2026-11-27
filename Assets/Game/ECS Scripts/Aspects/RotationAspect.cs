using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System;
using UnityEngine;

public sealed class RotationAspect : IProtoAspect
{
    public ProtoPool<CubeRotationComponent> CubeRotationPool;

    private ProtoWorld _world;

    public void Init(ProtoWorld world)
    {
        _world = world;

        world.AddAspect(this);

        CubeRotationPool = new ProtoPool<CubeRotationComponent>();
        world.AddPool(CubeRotationPool);
    }

    public void PostInit()
    {
    }

    public ProtoWorld World()
    {
        return _world;
    }
}


// Компоненты аспекта пишем под аспектом
[ProtoUnityAuthoring]
public struct CubeRotationComponent
{
    public bool isRotating;
}