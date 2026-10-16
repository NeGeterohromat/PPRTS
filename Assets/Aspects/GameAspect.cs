using UnityEngine;
using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;

public class GameAspect : ProtoAspectInject
{
    public ProtoPool<Position> Positions;
    public ProtoPool<Production> Productions;
    public ProtoPool<Consumption> Consumptions;
    public ProtoPool<BuildingHealth> BuildingHealths;
}
