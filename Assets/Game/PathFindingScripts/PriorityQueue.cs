using System;
using System.Collections.Generic;
using UnityEngine;

// Простая реализация PriorityQueue (если нет в вашей версии .NET)
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement element, TPriority priority)> _heap = new();
    public int Count => _heap.Count;
    public void Enqueue(TElement element, TPriority priority)
    {
        _heap.Add((element, priority));
        int i = _heap.Count - 1;
        while (i > 0)
        {
            int p = (i - 1) / 2;
            if (_heap[p].priority.CompareTo(_heap[i].priority) <= 0) break;
            (_heap[p], _heap[i]) = (_heap[i], _heap[p]);
            i = p;
        }
    }
    public TElement Dequeue()
    {
        var result = _heap[0].element;
        _heap[0] = _heap[^1];
        _heap.RemoveAt(_heap.Count - 1);
        int i = 0;
        while (true)
        {
            int l = 2 * i + 1, r = 2 * i + 2, smallest = i;
            if (l < _heap.Count && _heap[l].priority.CompareTo(_heap[smallest].priority) < 0) smallest = l;
            if (r < _heap.Count && _heap[r].priority.CompareTo(_heap[smallest].priority) < 0) smallest = r;
            if (smallest == i) break;
            (_heap[i], _heap[smallest]) = (_heap[smallest], _heap[i]);
            i = smallest;
        }
        return result;
    }
}