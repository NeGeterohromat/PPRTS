using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class GameStartup : MonoBehaviour
{
    private IProtoSystems _systems;

    void Start()
    {
        var aspect = new GameAspect();
        var world = new ProtoWorld(aspect);

        _systems = new ProtoSystems(world);

        _systems
            .AddModule(new AutoInjectModule())
            .AddSystem(new ProductionSystem())
            .AddSystem(new ConsumptionSystem())
            .Init();

        ref var position = ref aspect.Positions.NewEntity(out var sawmill);
        position = new Position { X = 0, Y = 0 };

        ref var health = ref aspect.BuildingHealths.Add(sawmill);
        health = new BuildingHealth { Current = 100, Max = 100 };

        ref var production = ref aspect.Productions.Add(sawmill);
        production = new Production
        {
            OutputResource = ResourceType.Wood,
            BaseRate = 5,
            CycleTime = 2,
            Progress = 0
        };

    }

    void Update()
    {
        _systems.Run();
    }

    void OnDestroy()
    {
        _systems.Destroy();
    }
}