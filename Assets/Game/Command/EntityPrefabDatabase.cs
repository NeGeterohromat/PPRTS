using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System.Collections.Generic;
using UnityEngine;

public class EntityPrefabDatabase : IProtoInitService
{
    private Dictionary<EntityType, GameObject> _prefabs;

    public void Init(IProtoSystems systems)
    {
        Load();
    }

    private void Load()
    {
        _prefabs = new Dictionary<EntityType, GameObject>();

        EntityToPrefab[] entities =
            Resources.LoadAll<EntityToPrefab>("Entities");

        foreach (EntityToPrefab entity in entities)
        {
            if (entity == null || entity.Prefab == null)
                continue;

            if (_prefabs.ContainsKey(entity.Type))
            {
                Debug.LogWarning($"Duplicate EntityType: {entity.Type}");
                continue;
            }

            _prefabs.Add(entity.Type, entity.Prefab);
        }
        Debug.Log("Loaded");
    }

    public GameObject Get(EntityType type)
    {
        if (_prefabs.TryGetValue(type, out GameObject prefab))
            return prefab;

        Debug.LogError($"Prefab for EntityType {type} not found!");
        return null;
    }
}