using UnityEngine;

[CreateAssetMenu(fileName = "EntityToPrefab", menuName = "Scriptable Objects/EntityToPrefab")]
public class EntityToPrefab : ScriptableObject
{
    public EntityType Type;
    public GameObject Prefab;
}
