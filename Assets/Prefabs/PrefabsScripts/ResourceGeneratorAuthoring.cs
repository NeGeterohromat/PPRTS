using UnityEngine;

public class ResourceGeneratorAuthoring : MonoBehaviour
{
    [Header("Настройки здания")]
    public ResourceType ResourceToProduce = ResourceType.Wood;
    public float BaseRate = 5f;
    public float CycleTime = 2f;

    void Start()
    {
        GameAspect aspect = EcsStartup.Instance.Aspect;

        ref var pos = ref aspect.Positions.NewEntity(out var entity);
        pos = new PositionComponent { X = transform.position.x, Y = transform.position.z };

        ref var health = ref aspect.BuildingHealths.Add(entity);
        health = new BuildingHealthComponent { Current = 100, Max = 100 };

        ref var prod = ref aspect.Productions.Add(entity);
        prod = new ProductionComponent
        {
            OutputResource = ResourceToProduce,
            BaseRate = BaseRate,
            CycleTime = CycleTime,
            Progress = 0
        };

        Debug.Log($"Здание {gameObject.name} создано в ECS и начинает добычу {ResourceToProduce}");
    }
}