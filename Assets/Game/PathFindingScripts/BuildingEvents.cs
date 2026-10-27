using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class BuildingEvents
{
    [DI] private MovementAspect aspect;

    // Вызывайте этот метод извне после изменения построек
    public void UpdateBuildingData()
    {
        foreach (var entity in aspect.InMovementIterator)
        {
            if (!aspect.NeedCalculatePathPool.Has(entity))
            {
                aspect.NeedCalculatePathPool.Add(entity);
            }
        }
    }
}
