using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LightTransport;

public class EntityRegistrySystem : IProtoInitSystem, IProtoRunSystem
{
    private ProtoWorld _world;
    private UnityUpdateAspect _aspect;

    [DI] private IComandPopper _commandQueue;
    // Словарь для быстрого поиска
    private Dictionary<int, ProtoPackedEntityWithWorld> GameIdToEntity;

    public void Init(IProtoSystems systems) 
    {
        _world = systems.World();
        GameIdToEntity = new Dictionary<int, ProtoPackedEntityWithWorld>();

        _aspect = (UnityUpdateAspect)_world.Aspect(typeof(UnityUpdateAspect));
    }

    public void Run()
    {
        // В LeoECS Lite/Complex можно использовать фильтры с включением/исключением,
        // но для простоты и скорости в RTS лучше использовать события или 
        // помечать "новых" юнитов специальным компонентом-флагом.

        // Пример с компонентом-флагом "Только что создан":
        foreach (var entity in _aspect.NewlyCreatedComandableEntityIterator)
        {
            var packed = _world.PackEntityWithWorld(entity);
            ref var tag = ref _aspect.ComandableEntityTagPool.Add(entity);
            tag.Id = packed.GetHashCode();
            _aspect.NewlyCreatedComandableEntityEventPool.Get(entity).Obj.GetComponent<EntityID>().ID = tag.Id;
            GameIdToEntity[tag.Id] = packed;
            _aspect.NewlyCreatedComandableEntityEventPool.Del(entity); // Убираем флаг, чтобы не обрабатывать снова
        }

        // ОБЯЗАТЕЛЬНО: Чистим словарь от мертвых сущностей!
        // Иначе словарь будет расти и хранить "мусор".
        foreach (var entity in _aspect.NewlyDeletedComandableEntityIterator)
        {
            ref var tag = ref _aspect.ComandableEntityTagPool.Get(entity);
            GameIdToEntity.Remove(tag.Id);
            _aspect.NewlyDeletedComandableEntityEventPool.Del(entity);
            _aspect.ComandableEntityTagPool.Del(entity);
        }

        // Command Execute
        while (_commandQueue.HasElements())
        {
            _commandQueue.Pop().Execute(GameIdToEntity);
        }
    }
}