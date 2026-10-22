using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class ProductionSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private GameAspect _aspect;
    private ProtoIt _producers;
    private ProtoIt _players;

    public void Init(IProtoSystems systems)
    {
        _producers = new ProtoIt(new[] { typeof(ProductionComponent), typeof(BuildingHealthComponent) });
        _producers.Init(systems.World());

        _players = new ProtoIt(new[] { typeof(PlayerTagComponent), typeof(PlayerInventoryComponent) });
        _players.Init(systems.World());
    }

    public void Run()
    {
        float dt = Time.deltaTime;

        foreach (ProtoEntity entity in _producers)
        {
            ref var prod = ref _aspect.Productions.Get(entity);
            ref var health = ref _aspect.BuildingHealths.Get(entity);

            if (health.Current <= 0) continue;

            prod.Progress += dt;

            if (prod.Progress >= prod.CycleTime)
            {
                prod.Progress -= prod.CycleTime;

                foreach (var player in _players)
                {
                    ref var inv = ref _aspect.Inventories.Get(player);

                    switch (prod.OutputResource)
                    {
                        case ResourceType.Wood: inv.Wood += prod.BaseRate; break;
                        case ResourceType.Coal: inv.Coal += prod.BaseRate; break;
                        case ResourceType.Food: inv.Food += prod.BaseRate; break;
                    }

                    Debug.Log($"[Сбор ресурсов] Игрок получил {prod.BaseRate} {prod.OutputResource}. " +
                              $"Всего в инвентаре: Дерево={inv.Wood}, Уголь={inv.Coal}");
                }
            }
        }
    }
}