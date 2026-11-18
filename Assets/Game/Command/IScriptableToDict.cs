using System;
using UnityEngine;

public interface IScriptableToDict
{
    public int GetTypeValue();
    public Type GetEnumType();
    public GameObject GetPrefab();
}