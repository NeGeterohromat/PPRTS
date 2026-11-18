using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityToPrefab", menuName = "Scriptable Objects/EntityToPrefab")]
public class EntityToPrefab : ScriptableObject, IScriptableToDict
{
    public EntityType Type;
    public GameObject Prefab;

    public GameObject GetPrefab() => Prefab;

    public int GetTypeValue() => (int)Type;

    public Type GetEnumType() => typeof(EntityType);
}
