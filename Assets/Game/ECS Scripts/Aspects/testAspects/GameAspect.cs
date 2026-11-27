using UnityEngine;
using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using JetBrains.Annotations;
using System.Collections.Generic;

public class GameAspect : ProtoAspectInject
{
    public ProtoPool<PositionComponent> Positions;
    public ProtoPool<ProductionComponent> Productions;
    public ProtoPool<ConsumptionComponent> Consumptions;
    public ProtoPool<BuildingHealthComponent> BuildingHealths;

    public ProtoPool<WarehouseComponent> WarehousePool;
    public ProtoPool<PlacableBuildingComponent> PlacableBuildingPool;
    public ProtoPool<InBuildingProcessTagComponent> InBuildingProcessTagPool;

    public ProtoPool<PlayerTagComponent> Players;
    public ProtoPool<PlayerInventoryComponent> Inventories;
    public ProtoPool<OwnerComponent> OwnerComponents;

    public ProtoIt PlayerIterator = new(new[] { typeof(PlayerTagComponent), typeof(PlayerInventoryComponent) });
    public ProtoIt InBuildingProcessIterator = new(new[] { typeof(InBuildingProcessTagComponent) });
}

[ProtoUnityAuthoring]
public struct PositionComponent
{
    public float X;
    public float Y;
}

[ProtoUnityAuthoring]
public struct InBuildingProcessTagComponent 
{
    public float ProcessTime;
    public float CurrentProgress;
    public float TimeForOneBuilder;
    public bool isBuildersCame;
    public ProtoPackedEntityWithWorld[] Builders;
}

[ProtoUnityAuthoring]
public struct BuildingHealthComponent
{
    public float Current;
    public float Max;
}

[ProtoUnityAuthoring]
public struct ProductionComponent
{
    public ResourceType OutputResource;
    public float BaseRate;
    public float Progress;
    public float CycleTime;

    public float CurrentResourseAmount;
    public float MaxResourseAmount;
}

[ProtoUnityAuthoring]
public struct WarehouseComponent
{
    public ResourceType Resourse;
    public float CurrentAmount;
    public float MaxAmount;
}

[ProtoUnityAuthoring]
public struct PlacableBuildingComponent
{
    public float Price;
}

public struct ConsumptionComponent
{
    public ResourceType InputResource;
    public float RatePerSecond;
}

public struct PlayerTagComponent
{
    public int ID;
}

[ProtoUnityAuthoring]
public struct OwnerComponent
{
    public int OwnerID;
}

// Этот компонент только для отображения в UI. В нём собираются все ресурсы со всех добытчиков и складов.
public struct PlayerInventoryComponent
{
    public float Money;
    public float Wood;
    public float Coal;
    public float Food;
    public float Oil; 
    public float Gas;
}

public enum ResourceType
{
    None,
    Wood,
    Coal,
    Steel,
    Food,
    Oil, 
    Gas
}