using Leopotam.EcsProto;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public sealed class EcsStartup : MonoBehaviour
{
    private IProtoSystems _systems;

    private void Start()
    {
        ProtoWorld world = new ProtoWorld(
            new RotationAspect()
        );

        _systems = new ProtoSystems(world)
            .AddModule(new UnityModule())
            .AddSystem(new CubeRotationSystem());

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