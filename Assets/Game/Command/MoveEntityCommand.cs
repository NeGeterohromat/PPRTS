using Leopotam.EcsProto.QoL;
using UnityEngine;

public class MoveEntityCommand : ICommand
{
    private ProtoPackedEntityWithWorld entity;
    private Vector3 position;
    public MoveEntityCommand(ProtoPackedEntityWithWorld entity,Vector3 position)
    {
        this.entity = entity;
        this.position = position;
    }
    public void Execute()
    {
        entity.TryUnpack(out var world, out var en);
        var twoDPos = new Vector2(position.x, position.z);
        var aspect = world.Aspect<MovementAspect>();
        if (!aspect.NeedCalculatePathPool.Has(en))
            aspect.NeedCalculatePathPool.Add(en);
        if (!aspect.MovementGoalPool.Has(en))
            aspect.MovementGoalPool.Add(en);
        ref var moveComp = ref aspect.MovementGoalPool.Get(en);
        moveComp.GoalPosition = twoDPos;
    }
}
