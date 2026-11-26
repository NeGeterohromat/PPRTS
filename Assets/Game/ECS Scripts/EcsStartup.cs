using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    public static CommandQueue InstanceQueue { get; private set; }

    private IProtoSystems _systems;
    private InputSystem_Actions actions;
    [SerializeField] private MouseSelector mouseSelector;

    private void Awake()
    {
        ProtoWorld world = new ProtoWorld(new BaseAspect());

        var playerID = new ThisPlayerID() { ID = 1 };

        CommandQueue fullQueue = new CommandQueue();
        InstanceQueue = fullQueue; 
        ICommandAdder adder = fullQueue;
        ICommandPopper popper = fullQueue;
        actions = new();
        actions.Enable();

        _systems = new ProtoSystems(world)
            .AddModule(new AutoInjectModule(true))
            .AddService(playerID)
            .AddService(adder, typeof(ICommandAdder))
            .AddService(popper, typeof(ICommandPopper))
            .AddService(actions, typeof(InputSystem_Actions))
            .AddService(mouseSelector)
            .InitHere<MouseSelector>()
            .AddService(new SelectionManager())
            .AddService(new MultiSelectionController())
            .AddService(new ObjectSelectionController())
            .AddService(new BuildingPlacementController())
            .InitHere<BuildingPlacementController>()
            .AddService(new BuildingData())
            .AddService(new PathfindingService())
            .AddService(new BuildingEvents())
            .AddModule(new MovementModule())
            .AddModule(new UnityModule())
            .AddModule(new CameraMovementModule())
            .AddModule(new CubeRotationModule())
            .AddModule(new UnityUpdateModule())


            .AddSystem(new ProductionSystem())
            .AddSystem(new ConsumptionSystem())

            .AddSystem(new OppositionAISystem());

        CreatePlayer(1);
        CreatePlayer(2);
        _systems.Init();

        PrefabDatabase.Init();
    }

    private void CreatePlayer(int id)
    {
        var aspect = _systems.World().Aspect<GameAspect>();
        ref var tag = ref aspect.Players.NewEntity(out var player);
        ref var inv = ref aspect.Inventories.Add(player);
        tag.ID = id;
        inv.Wood = 0;
        inv.Coal = 0;
        inv.Food = 0;
        inv.Money = 100_000;
        Debug.Log($"Игрок {id} создан");
    }

    private void Update() => _systems?.Run();

    private void OnDestroy()
    {
        _systems?.Destroy();
        _systems = null;
    }
}

public struct ThisPlayerID
{
    public int ID;
}