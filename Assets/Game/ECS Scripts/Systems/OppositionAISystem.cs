using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Linq.Expressions;
using UnityEngine;

public class OppositionAISystem : IProtoRunSystem, IProtoInitSystem
{
    private readonly int aiID = 2;

    [DI] private ICommandAdder adder;
    [DI] private GameAspect aspect;

    private ProtoWorld world;

    private ProtoIt OwnerComponentIterator = new(new[] { typeof(OwnerComponent) });

    private int spawnTimeInSeconds = 10;
    private int goTimeInSeconds = 15;
    private bool isSpawned = false;
    private bool isGoing = false;
    private ProtoPackedEntityWithWorld aiEntity;
    private bool isEntity = false;

    public void Init(IProtoSystems systems)
    {
        world = systems.World();
        OwnerComponentIterator.Init(world);
    }

    public void Run()
    {
        if (!isSpawned && Time.realtimeSinceStartup >= spawnTimeInSeconds) 
        {
            isSpawned = true;
            adder.Add(new CreateEntityCommand(EntityType.RotatingCircle, new Vector3(5, 0, 5), aiID));
        }

        if (!isEntity && isSpawned) 
        {
            foreach (var en in OwnerComponentIterator) 
            {
                ref var ownerComponent = ref aspect.OwnerComponents.Get(en);
                if (ownerComponent.OwnerID == aiID)
                {
                    isEntity = true;
                    aiEntity = world.PackEntityWithWorld(en);
                }
            }
        }

        if (!isGoing && isSpawned && isEntity && Time.realtimeSinceStartup >= goTimeInSeconds) 
        {
            isGoing = true;
            adder.Add(new MoveEntityCommand(aiEntity, new Vector3(9.3f, 0, 3.2f), aiID));
        }
    }
}
