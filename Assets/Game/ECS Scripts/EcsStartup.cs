using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    public static EcsStartup Instance { get; private set; }
    public GameAspect Aspect { get; private set; }

    private IProtoSystems _systems;

    private void Awake()
    {
        Instance = this;

        Aspect = new GameAspect();
        ProtoWorld world = new ProtoWorld(Aspect);

        _systems = new ProtoSystems(world)
            .AddModule(new AutoInjectModule())
            .AddModule(new UnityModule())
            .AddModule(new CubeRotationModule()) 
            .AddModule(new UnityUpdateModule())  

            .AddSystem(new ProductionSystem())
            .AddSystem(new ConsumptionSystem());

        CreatePlayer();
        _systems.Init();
    }

    private void CreatePlayer()
    {
        ref var tag = ref Aspect.Players.NewEntity(out var player);
        ref var inv = ref Aspect.Inventories.Add(player);
        inv.Wood = 0;
        inv.Coal = 0;
        inv.Food = 0;
        Debug.Log("Игрок создан");
    }

    private void Update() => _systems?.Run();

    private void OnDestroy()
    {
        _systems?.Destroy();
        _systems = null;
    }
}