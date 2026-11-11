using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using UnityEngine;

public class CreateEntityCommand : ICommand
{
    private GameObject Prefab;
    private Vector3 position;
    private int senderId;
    public CreateEntityCommand(GameObject Prefab, Vector3 position, int senderId) 
    {
        this.Prefab = Prefab;
        this.senderId = senderId;
        this.position = position;
    }

    public void Execute()
    {
        var obj = GameObject.Instantiate(Prefab, position, Quaternion.identity);
        var entity = obj.GetComponent<ProtoUnityAuthoring>().Entity();

        entity.TryUnpack(out var world, out var en);
        var playerAspect = world.Aspect<GameAspect>();
        var updateAspect = world.Aspect<UnityUpdateAspect>();
        if (playerAspect.OwnerComponents.Has(en))
        {
            ref var ownerComponent = ref playerAspect.OwnerComponents.Get(en);
            ownerComponent.OwnerID = senderId;
        }
        if (updateAspect.TransformDataPool.Has(en))
        {
            ref var transform = ref updateAspect.TransformDataPool.Get(en);
            transform.Position = position;
        }
    }
}
