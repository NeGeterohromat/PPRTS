using Leopotam.EcsProto.QoL;
using UnityEngine;

public class MoveEntityCommand : ICommand
{
    private ProtoPackedEntityWithWorld entity;
    private Vector3 position;
    private int senderId;
    public MoveEntityCommand(ProtoPackedEntityWithWorld entity,Vector3 position, int senderId)
    {
        this.entity = entity;
        this.position = position;
        this.senderId = senderId;
    }
    public void Execute()
    {
        entity.TryUnpack(out var world, out var en);
        var twoDPos = new Vector2(position.x, position.z);
        var aspect = world.Aspect<MovementAspect>();
        var playerAspect = world.Aspect<GameAspect>();
        // Owner is incorrect
        if (!playerAspect.OwnerComponents.Has(en) || playerAspect.OwnerComponents.Get(en).OwnerID != senderId) return;

        if (!aspect.NeedCalculatePathPool.Has(en))
            aspect.NeedCalculatePathPool.Add(en);
        if (!aspect.MovementGoalPool.Has(en))
            aspect.MovementGoalPool.Add(en);
        ref var moveComp = ref aspect.MovementGoalPool.Get(en);
        moveComp.GoalPosition = twoDPos;
    }
}
