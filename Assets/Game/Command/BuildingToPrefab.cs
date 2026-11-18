using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingToPrefab", menuName = "Scriptable Objects/BuildingToPrefab")]
public class BuildingToPrefab : ScriptableObject, IScriptableToDict
{
    public GameObject Prefab;
    public BuildingType Type;

    public GameObject GetPrefab() => Prefab;

    public int GetTypeValue() => (int)Type;

    public Type GetEnumType() => typeof(BuildingType);
}