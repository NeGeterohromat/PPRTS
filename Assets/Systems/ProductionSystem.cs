using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine; 

public class ProductionSystem : IProtoInitSystem, IProtoRunSystem
{
    [DI] private GameAspect _aspect; 
    private ProtoIt _producers;

    public void Init(IProtoSystems systems)
    {
        _producers = new ProtoIt(new[] { typeof(Production), typeof(BuildingHealth) });
        _producers.Init(systems.World());  
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
                Debug.Log($"Произведено {prod.BaseRate} единиц ресурса {prod.OutputResource}");
            }
        }
    }
}