using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System;
using UnityEngine;

public class UnityUpdateAspect : IProtoAspect
{
    private ProtoWorld _world;

    public ProtoPool<NeedTransformUpdateByCameraEventComponent> NeedTransformUpdateByCameraEventPool = new();
    public ProtoPool<NeedTransformUpdateEventComponent> NeedTransformUpdateEventPool = new();
    public ProtoPool<TransformDataComponent> TransformDataPool = new();
    public ProtoPool<GameObjectComponent> GameObjectPool = new();
    public ProtoPool<NewlyCreatedComandableEntityEventComponent> NewlyCreatedComandableEntityEventPool = new();
    public ProtoPool<NewlyDeletedComandableEntityEventComponent> NewlyDeletedComandableEntityEventPool = new();
    public ProtoPool<ComandableEntityTagComponent> ComandableEntityTagPool = new();

    public ProtoIt TransformUpdateIterator;
    public ProtoIt TransformDataIterator;
    public ProtoIt NewlyCreatedComandableEntityIterator;
    public ProtoIt NewlyDeletedComandableEntityIterator;

    public void Init(ProtoWorld world)
    {
        _world = world;

        world.AddAspect(this);

        world.AddPool(NeedTransformUpdateByCameraEventPool);
        world.AddPool(NeedTransformUpdateEventPool);
        world.AddPool(TransformDataPool);
        world.AddPool(GameObjectPool);
        world.AddPool(NewlyCreatedComandableEntityEventPool);
        world.AddPool(NewlyDeletedComandableEntityEventPool);
        world.AddPool(ComandableEntityTagPool);

        TransformUpdateIterator = new(new[] 
        { 
            typeof(NeedTransformUpdateByCameraEventComponent),  
            typeof(NeedTransformUpdateEventComponent),
            typeof(TransformDataComponent),
            typeof(GameObjectComponent)
        });

        TransformDataIterator = new(new[]
        {
            typeof(TransformDataComponent)
        });
        NewlyCreatedComandableEntityIterator = new(new[] { typeof(NewlyCreatedComandableEntityEventComponent) });
        NewlyDeletedComandableEntityIterator = new(new[] { typeof(NewlyDeletedComandableEntityEventComponent) });
    }

    public void PostInit()
    {
        TransformUpdateIterator.Init(_world);
        TransformDataIterator.Init(_world);
        NewlyCreatedComandableEntityIterator.Init(_world);
        NewlyDeletedComandableEntityIterator.Init(_world);
    }

    public ProtoWorld World()
    {
        return _world;
    }
}

public struct NeedTransformUpdateByCameraEventComponent { } // Близко ли камера

public struct NeedTransformUpdateEventComponent { } // А менялась ли вообще позиция объекта

[ProtoUnityAuthoring]
public struct TransformDataComponent
{
    public Vector3 Position;
    public Quaternion Rotation;
}

[Serializable, ProtoUnityAuthoring]
public struct GameObjectComponent : IProtoUnityAuthoring
{
    // Ссылка на Unity-объект, соответствующий ECS-сущности с этим компонентом.
    public GameObject Obj;

    public void Authoring(
        in ProtoPackedEntityWithWorld entity,
        GameObject go)
    {
        Obj = go;
    }
}

[ProtoUnityAuthoring]
public struct NewlyCreatedComandableEntityEventComponent : IProtoUnityAuthoring 
{
    public GameObject Obj;

    public void Authoring(
        in ProtoPackedEntityWithWorld entity,
        GameObject go)
    {
        Obj = go;
    }
}

public struct ComandableEntityTagComponent 
{
    public int Id;
}

public struct NewlyDeletedComandableEntityEventComponent { }

