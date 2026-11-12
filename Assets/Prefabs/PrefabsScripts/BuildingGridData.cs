using UnityEngine;

public class BuildingGridData : MonoBehaviour
{
    [Tooltip("Сколько клеток занимает здание по X и Z")]
    public Vector2Int GridSize = new Vector2Int(1, 1);

    private void Awake()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            GridSize = new Vector2Int(
                Mathf.Max(1, Mathf.RoundToInt(boxCollider.size.x)),
                Mathf.Max(1, Mathf.RoundToInt(boxCollider.size.z))
            );
        }
    }
}