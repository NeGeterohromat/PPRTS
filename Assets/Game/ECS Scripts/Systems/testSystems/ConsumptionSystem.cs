using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class ConsumptionSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private GameAspect _aspect;
    private ProtoIt _consumers;

    public void Init(IProtoSystems systems)
    {
        _consumers = new ProtoIt(new[] {
            typeof(ConsumptionComponent),
            typeof(BuildingHealthComponent)
        });
        _consumers.Init(systems.World());
    }

    public void Run()
    {
        float dt = Time.deltaTime;

        foreach (ProtoEntity entity in _consumers)
        {
            ref var consumption = ref _aspect.Consumptions.Get(entity);
            ref var health = ref _aspect.BuildingHealths.Get(entity);

            if (health.Current <= 0) continue;

            float needed = consumption.RatePerSecond * dt;

            Debug.Log($"Здание потребило {needed:F2} {consumption.InputResource}");
        }
    }
}