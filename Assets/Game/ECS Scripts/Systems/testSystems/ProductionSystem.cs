using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class ProductionSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private GameAspect _aspect;
    private ProtoIt _producers;
    private ProtoIt _players;
    [DI] private ThisPlayerID playerID;

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
                    ref var p = ref _aspect.Players.Get(player);
                    if (p.ID != playerID.ID) continue;

                    ref var inv = ref _aspect.Inventories.Get(player);

                    var isResourseMined = false;

                    switch (prod.OutputResource)
                    {
                        case ResourceType.Wood:
                            if (prod.CurrentResourseAmount + prod.BaseRate <= prod.MaxResourseAmount)
                            {
                                isResourseMined = true;
                                inv.Wood += prod.BaseRate;
                                prod.CurrentResourseAmount += prod.BaseRate;
                            }
                            break;
                        case ResourceType.Coal:
                            if (prod.CurrentResourseAmount + prod.BaseRate <= prod.MaxResourseAmount)
                            {
                                isResourseMined = true;
                                inv.Coal += prod.BaseRate;
                                prod.CurrentResourseAmount += prod.BaseRate;
                            }
                            break;
                        //case ResourceType.Steel: inv.Steel += prod.BaseRate; break;
                        case ResourceType.Oil:
                            if (prod.CurrentResourseAmount + prod.BaseRate <= prod.MaxResourseAmount)
                            {
                                isResourseMined = true;
                                inv.Oil += prod.BaseRate;
                                prod.CurrentResourseAmount += prod.BaseRate;
                            }
                            break;
                        case ResourceType.Gas:
                            if (prod.CurrentResourseAmount + prod.BaseRate <= prod.MaxResourseAmount)
                            {
                                isResourseMined = true;
                                inv.Gas += prod.BaseRate;
                                prod.CurrentResourseAmount += prod.BaseRate;
                            }
                            break;
                    }
                    if (isResourseMined)
                        Debug.Log($"[Сбор] Здание добыло {prod.BaseRate} {prod.OutputResource}. " +
                              $"В инвентаре: Нефть={inv.Oil}, Газ={inv.Gas}, Уголь={inv.Coal}");
                    else
                        Debug.Log($"У здания {prod.OutputResource} кончилось место во внутреннем хранилище");
                }
            }
        }
    }
}