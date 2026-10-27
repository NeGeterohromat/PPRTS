using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    private IProtoSystems _systems;
    private InputSystem_Actions actions;
    [SerializeField] private MouseSelector mouseSelector;

    private void Awake()
    {
        ProtoWorld world = new ProtoWorld(new BaseAspect());

        CommandQueue fullQueue = new CommandQueue();
        ICommandAdder adder = fullQueue;
        ICommandPopper popper = fullQueue;
        actions = new();
        actions.Enable();

        _systems = new ProtoSystems(world)
            .AddModule(new AutoInjectModule(true))
            .AddService(adder,typeof(ICommandAdder))
            .AddService(popper,typeof(ICommandPopper))
            .AddService(actions, typeof(InputSystem_Actions))            
            .AddService(mouseSelector)
            .AddService(new BuildingData())
            .AddService(new PathfindingService())
            .AddService(new BuildingEvents())
            .AddModule(new MovementModule())
            .AddModule(new UnityModule())
            .AddModule(new CameraMovementModule())
            .AddModule(new CubeRotationModule())
            .AddModule(new UnityUpdateModule())

            .AddSystem(new ProductionSystem())
            .AddSystem(new ConsumptionSystem());

        CreatePlayer();
        _systems.Init();
        mouseSelector.Init(_systems);
    }

    private void CreatePlayer()
    {
        var aspect = _systems.World().Aspect<GameAspect>();
        ref var tag = ref aspect.Players.NewEntity(out var player);
        ref var inv = ref aspect.Inventories.Add(player);
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