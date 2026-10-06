using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System;
using UnityEngine;

public sealed class RotationAspect : IProtoAspect
{
    public ProtoPool<CubeRotationComponent> CubeRotationPool;
    public ProtoIt CubeRotationIterator;

    private ProtoWorld _world;

    public void Init(ProtoWorld world)
    {
        _world = world;

        world.AddAspect(this);

        CubeRotationPool = new ProtoPool<CubeRotationComponent>();
        world.AddPool(CubeRotationPool);

        CubeRotationIterator = new ProtoIt(
            new[]
            {
                typeof(CubeRotationComponent)
            }
        );
    }

    public void PostInit()
    {
        CubeRotationIterator.Init(_world);
    }

    public ProtoWorld World()
    {
        return _world;
    }
}


// Компоненты аспекта пишем под аспектом
[Serializable, ProtoUnityAuthoring]
public struct CubeRotationComponent : IProtoUnityAuthoring
{
    public bool isRotating;

    // Ссылка на Unity-объект, соответствующий ECS-сущности с этим компонентом.
    public GameObject View;

    public void Authoring(
        in ProtoPackedEntityWithWorld entity,
        GameObject go)
    {
        View = go;
    }
}