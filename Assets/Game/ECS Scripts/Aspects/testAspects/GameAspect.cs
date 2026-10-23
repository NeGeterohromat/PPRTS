using UnityEngine;
using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;

public class GameAspect : BaseAspect
{
    public ProtoPool<PositionComponent> Positions;
    public ProtoPool<ProductionComponent> Productions;
    public ProtoPool<ConsumptionComponent> Consumptions;
    public ProtoPool<BuildingHealthComponent> BuildingHealths;

    public ProtoPool<PlayerTagComponent> Players;
    public ProtoPool<PlayerInventoryComponent> Inventories;
}

[ProtoUnityAuthoring]
public struct PositionComponent
{
    public float X;
    public float Y;
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
}

public struct ConsumptionComponent
{
    public ResourceType InputResource;
    public float RatePerSecond;
}

public struct PlayerTagComponent
{
  
}

public struct PlayerInventoryComponent
{
    public float Wood;
    public float Coal;
    public float Food;
}

public enum ResourceType
{
    None,
    Wood,
    Coal,
    Steel,
    Food
}