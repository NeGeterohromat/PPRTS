using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto.Unity;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BuildingProcessSystem : IProtoRunSystem
{
    [DI] private GameAspect _aspect;
    [DI] private UnityUpdateAspect _updateAspect;
    [DI] private MovementAspect _movementAspect;

    public void Run()
    {
        foreach (var entity in _aspect.InBuildingProcessIterator)
        {
            ref var proc = ref _aspect.InBuildingProcessTagPool.Get(entity);

            if (!proc.isBuildersCame)
            {
                proc.isBuildersCame = proc.Builders
                    .Select(u =>
                    {
                        u.TryUnpack(out var _, out var e);
                        return !_movementAspect.MovementGoalPool.Has(e);
                    })
                    .All(t => t);
                continue;
            }

            proc.CurrentProgress += Time.deltaTime;

            if (proc.CurrentProgress >= proc.ProcessTime)
            {
                Debug.Log(string.Join(' ', proc.Builders));

                foreach (var unit in proc.Builders)
                {
                    unit.TryUnpack(out var _, out var e);
                    ref var comm = ref _updateAspect.HasCommandTagPool.Get(e);
                    _updateAspect.HasCommandTagPool.Del(e);

                }
                _aspect.InBuildingProcessTagPool.Del(entity);
                Debug.Log($"Building process is Ended");
            }
        }
    }
}