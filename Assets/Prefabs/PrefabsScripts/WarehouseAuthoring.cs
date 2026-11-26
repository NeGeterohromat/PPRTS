using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public class WarehouseAuthoring : ProtoUnityAuthoring
{
    [Header("Настройки здания")]
    public float MaxResourceAmount = 40f;

    protected override void Start()
    {
        base.Start();

        Entity().TryUnpack(out var world, out var entity);

        var aspect = world.Aspect<GameAspect>();

        ref var pos = ref aspect.Positions.Get(entity);
        pos = new PositionComponent { X = transform.position.x, Y = transform.position.z };

        ref var health = ref aspect.BuildingHealths.Get(entity);
        health = new BuildingHealthComponent { Current = 100, Max = 100 };

        ref var prod = ref aspect.WarehousePool.Get(entity);
        prod = new WarehouseComponent
        {
            Resourse = ResourceType.None,
            CurrentAmount = 0,
            MaxAmount = MaxResourceAmount
        };
    }
}