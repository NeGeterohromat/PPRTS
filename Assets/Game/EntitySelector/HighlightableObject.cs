using UnityEngine;

public class HighlightableObject : MonoBehaviour
{
    [Header("Настройки подсветки")]
    public Color highlightColor = Color.yellow; // Цвет при наведении/клике

    private Renderer objectRenderer;
    private Color originalColor;
    private Material uniqueMaterial;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // ВАЖНО: Создаем копию материала, чтобы не изменить цвет 
            // у всех объектов, использующих этот же материал в игре
            uniqueMaterial = new Material(objectRenderer.material);
            objectRenderer.material = uniqueMaterial;
            originalColor = uniqueMaterial.color;
        }
    }

    // Метод для включения подсветки
    public void Highlight()
    {
        if (uniqueMaterial != null)
        {
            uniqueMaterial.color = highlightColor;
        }
    }

    // Метод для возврата к исходному состоянию
    public void Unhighlight()
    {
        if (uniqueMaterial != null)
        {
            uniqueMaterial.color = originalColor;
        }
    }
}