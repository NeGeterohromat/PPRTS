using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public static class PrefabDatabase
{
    private static Dictionary<(Type, int), GameObject> _prefabs = new();

    public static void Init()
    {
        Load("Entities", typeof(EntityToPrefab));
        Load("Buildings", typeof(BuildingToPrefab));
    }

    private static void Load(string folder, Type scriptableType)
    {
        var objects =
            Resources.LoadAll(folder, scriptableType);

        foreach (IScriptableToDict obj in objects)
        {
            if (obj == null || obj.GetPrefab() == null)
                continue;

            if (_prefabs.ContainsKey((obj.GetEnumType(), obj.GetTypeValue())))
            {
                Debug.LogWarning($"Duplicate Type: {obj.GetEnumType()} {obj.GetTypeValue()}");
                continue;
            }

            _prefabs.Add((obj.GetEnumType(), obj.GetTypeValue()), obj.GetPrefab());
        }
        Debug.Log($"Loaded {folder}");
    }

    public static GameObject GetPrefab<T>(int type)
    {
        if (_prefabs.TryGetValue((typeof(T), type), out GameObject prefab))
            return prefab;

        Debug.LogError($"Prefab for Type {typeof(T)} and Value {type} not found!");
        return null;
    }
}