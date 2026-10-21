using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    private IProtoSystems _systems;
    private InputSystem_Actions actions;

    private void Start()
    {
        ProtoWorld world = new ProtoWorld(
            new BaseAspect()
        );

        CommandQueue fullQueue = new CommandQueue();
        IComandAdder adder = fullQueue;
        IComandPopper popper = fullQueue;
        actions = new();
        actions.Enable();

        _systems = new ProtoSystems(world)
            .AddService(adder,typeof(IComandAdder))
            .AddService(popper,typeof(IComandPopper))
            .AddService(actions, typeof(InputSystem_Actions))
            .AddModule(new AutoInjectModule())
            .AddModule(new UnityModule())
            .AddModule(new CameraMovementModule())
            .AddModule(new CubeRotationModule())
            .AddModule(new UnityUpdateModule());

        _systems.Init();
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