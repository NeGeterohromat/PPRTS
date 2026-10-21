using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    private IProtoSystems _systems;
    private InputSystem_Actions actions;
    [SerializeField] private MouseSelector mouseSelector;

    private void Start()
    {
        ProtoWorld world = new ProtoWorld(
            new BaseAspect()
        );

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
            .AddModule(new UnityModule())
            .AddModule(new CameraMovementModule())
            .AddModule(new CubeRotationModule())
            .AddModule(new UnityUpdateModule());

        _systems.Init();
        mouseSelector.Init(_systems);
    }

    private void Update()
    {
        _systems?.Run();
    }

    private void OnDestroy()
    {
        _systems?.Destroy();
        _systems = null;
    }
}