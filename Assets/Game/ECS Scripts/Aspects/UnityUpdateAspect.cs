using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System;
using UnityEngine;

public class UnityUpdateAspect : ProtoAspectInject
{
    public ProtoPool<VisibleByCameraTagComponent> VisibleByCameraTagPool;
    public ProtoPool<NeedTransformUpdateEventComponent> NeedTransformUpdateEventPool;
    public ProtoPool<TransformDataComponent> TransformDataPool;
    public ProtoPool<GameObjectComponent> GameObjectPool;
    public ProtoPool<HighlightedTagComponent> HighlightedTagPool;
    public ProtoPool<UnhighlightEventComponent> UnhighlightEventPool;

    public ProtoIt TransformUpdateIterator = new(new[]
        {
            typeof(VisibleByCameraTagComponent),
            typeof(NeedTransformUpdateEventComponent),
            typeof(TransformDataComponent),
            typeof(GameObjectComponent)
        });
    public ProtoIt TransformDataIterator = new(new[]
        {
            typeof(TransformDataComponent)
        });
    public ProtoIt VisibleEntityIterator = new (new[]
        {
            typeof(VisibleByCameraTagComponent),
            typeof(TransformDataComponent)
        });
    public ProtoIt HighlightedTagIterator = new(new[]
    {
        typeof(HighlightedTagComponent),
        typeof(GameObjectComponent)
    });
    public ProtoIt UnhighlightEventIterator = new(new[]
        {
            typeof(UnhighlightEventComponent),
            typeof(HighlightedTagComponent),
            typeof(GameObjectComponent)
        });
}

public struct VisibleByCameraTagComponent { } // Близко ли камера

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

public struct HighlightedTagComponent 
{
    public bool IsGOHighlighted;
}

public struct UnhighlightEventComponent { }

