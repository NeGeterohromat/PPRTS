using System;
using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class PathfindingService
{
    private const float CellLength = 1f; // замените на вашу константу

    [DI] private BuildingData buildingData;

    private struct Vertex : IEquatable<Vertex>
    {
        public int X, Z;
        public Vertex(int x, int z) { X = x; Z = z; }
        public bool Equals(Vertex other) => X == other.X && Z == other.Z;
        public override bool Equals(object obj) => obj is Vertex v && Equals(v);
        public override int GetHashCode() => HashCode.Combine(X, Z);
    }

    // Проверка: можно ли пройти по ребру между двумя соседними вершинами
    private bool IsEdgePassable(Vertex a, Vertex b)
    {
        int dx = b.X - a.X;
        int dz = b.Z - a.Z;

        // Соседние вершины (включая диагональные)
        // Ребро проходит между двумя клетками. Нужно проверить, что хотя бы одна из этих клеток свободна.
        // Для горизонтального/вертикального ребра: ребро лежит на границе двух клеток.
        // Для диагонального ребра: ребро проходит через угол, касаясь 4 клеток, но нам достаточно,
        // чтобы диагонально противоположные клетки (те, через которые ребро НЕ проходит) были свободны?
        // Фактически для диагонального движения из (x,z) в (x+1,z+1) ребро проходит через точку (x+1,z+1).
        // Оно разделяет две пары клеток. Юнит может пройти, если хотя бы одна из двух клеток,
        // которые ребро пересекает "по диагонали", свободна? Нет — ребро проходит точно через угол.
        // По условию: "Если две диагональные клетки пустые, то сущность может пройти по углу между этими клетками,
        // даже если две соседние диагональные клетки заняты". Это значит, что для прохода через угол
        // достаточно, чтобы две клетки, между которыми проходит угол, были свободны.
        // Проще: ребро разрешено, если ни одна из клеток, которые оно пересекает (или касается) не блокирует.
        // Для диагонали (dx=±1, dz=±1) ребро проходит через точку (max(a.X,b.X), max(a.Z,b.Z)).
        // Клетки, которые касаются этой точки: (a.X, a.Z), (b.X, a.Z), (a.X, b.Z), (b.X, b.Z).
        // Юнит может пройти, если хотя бы одна пара противоположных клеток свободна?
        // Упростим: проверим, что не заняты обе клетки, между которыми проходит ребро.
        // Для диагонали это клетки (b.X, a.Z) и (a.X, b.Z) — те, что не являются началом и концом.
        // Если обе заняты — проход невозможен (стена из двух построек).
        // Если хотя бы одна свободна — проход возможен.
        if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 1)
        {
            var c1 = buildingData.GetDataInPosition(new Vector2Int(b.X, a.Z));
            var c2 = buildingData.GetDataInPosition(new Vector2Int(a.X, b.Z));
            // Проход через угол разрешён, если хотя бы одна из этих клеток свободна
            return !c1.isBuilding || !c2.isBuilding;
        }

        // Горизонтальное или вертикальное ребро: ребро лежит на границе двух клеток.
        // Клетки: (minX, minZ) и (minX, minZ+1) для горизонтали, или (minX, minZ) и (minX+1, minZ) для вертикали.
        // Ребро проходимо, если хотя бы одна из двух клеток свободна.
        // Но по условию, если две постройки соприкасаются, юнит может пройти ровно по границе.
        // Значит, если обе клетки заняты — всё равно можно, если это разные постройки?
        // Нет: "сущность не может идти внутри постройки", но по границе между двумя постройками — может.
        // Если обе клетки заняты одной постройкой — граница внутренняя, идти нельзя.
        // Если обе заняты разными постройками — можно.
        // Проверим это через isBuilding и сравнение? BuildingData не даёт ID постройки.
        // Но у нас есть isUpBorder и т.д. Можно определить, является ли граница внешней.
        // Упрощение: если обе клетки заняты, проверим, является ли граница между ними "границей постройки"
        // хотя бы с одной стороны.
        // Для горизонтального ребра (dz=0): клетки (x, z-1) и (x, z). Ребро — это граница между ними.
        // Проходимо, если хотя бы одна клетка свободна ИЛИ граница является границей постройки (isUpBorder для нижней клетки или isDownBorder для верхней).
        int x1 = Mathf.Min(a.X, b.X);
        int z1 = Mathf.Min(a.Z, b.Z);

        if (dz == 0) // горизонтальное ребро
        {
            var lower = buildingData.GetDataInPosition(new Vector2Int(x1, z1 - 1));
            var upper = buildingData.GetDataInPosition(new Vector2Int(x1, z1));
            if (!lower.isBuilding || !upper.isBuilding) return true;
            // Обе заняты. Проверим, является ли граница внешней хотя бы для одной.
            // Если upper.isDownBorder == true, значит снизу от upper пусто или другая постройка? Нет, isDownBorder означает,
            // что клетка снизу не принадлежит той же постройке. Но она может быть занята другой постройкой.
            // Нам нужно: граница между lower и upper является границей постройки (т.е. они разные постройки или одна пустая).
            // isDownBorder у upper = true означает, что lower не часть той же постройки. Если lower занята, это другая постройка.
            // Значит, если upper.isDownBorder && lower.isBuilding, то это граница между двумя разными постройками — можно.
            // Аналогично lower.isUpBorder.
            return upper.isDownBorder || lower.isUpBorder;
        }
        else // вертикальное ребро (dx == 0)
        {
            var left = buildingData.GetDataInPosition(new Vector2Int(x1 - 1, z1));
            var right = buildingData.GetDataInPosition(new Vector2Int(x1, z1));
            if (!left.isBuilding || !right.isBuilding) return true;
            return right.isLeftBorder || left.isRightBorder;
        }
    }

    // Эвристика — манхэттенское расстояние (можно и евклидово, но для сетки вершин лучше манхэттен)
    private float Heuristic(Vertex a, Vertex b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Z - b.Z);
    }

    // Поиск ближайшей свободной точки, если цель внутри постройки
    public Vector2 ResolveGoal(Vector2 goal)
    {
        var cell = new Vector2Int(Mathf.FloorToInt(goal.x / CellLength), Mathf.FloorToInt(goal.y / CellLength));
        var data = buildingData.GetDataInPosition(cell);
        if (!data.isBuilding) return goal;

        // Ищем ближайшую свободную клетку по спирали
        for (int radius = 1; radius < 100; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dz) != radius) continue;
                    var neighbor = new Vector2Int(cell.x + dx, cell.y + dz);
                    var nd = buildingData.GetDataInPosition(neighbor);
                    if (!nd.isBuilding)
                    {
                        // Возвращаем центр этой клетки или ближайшую точку на её границе
                        return new Vector2((neighbor.x + 0.5f) * CellLength, (neighbor.y + 0.5f) * CellLength);
                    }
                }
            }
        }
        return goal; // fallback
    }

    // Основной поиск пути между двумя мировыми точками
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // Преобразуем мировые координаты в вершины сетки
        // Вершина (i,j) соответствует мировой точке (i * CellLength, j * CellLength)
        Vertex startV = new Vertex(
            Mathf.RoundToInt(start.x / CellLength),
            Mathf.RoundToInt(start.y / CellLength));
        Vertex endV = new Vertex(
            Mathf.RoundToInt(end.x / CellLength),
            Mathf.RoundToInt(end.y / CellLength));

        var open = new PriorityQueue<Vertex, float>();
        var cameFrom = new Dictionary<Vertex, Vertex>();
        var gScore = new Dictionary<Vertex, float>();
        var fScore = new Dictionary<Vertex, float>();

        gScore[startV] = 0;
        fScore[startV] = Heuristic(startV, endV);
        open.Enqueue(startV, fScore[startV]);

        var closed = new HashSet<Vertex>();

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current.Equals(endV))
            {
                // Восстанавливаем путь
                var path = new List<Vector2>();
                var node = current;
                while (cameFrom.ContainsKey(node))
                {
                    path.Add(new Vector2(node.X * CellLength, node.Z * CellLength));
                    node = cameFrom[node];
                }
                path.Add(new Vector2(startV.X * CellLength, startV.Z * CellLength));
                path.Reverse();
                return path;
            }

            closed.Add(current);

            // Соседи: 8 направлений
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;
                    var neighbor = new Vertex(current.X + dx, current.Z + dz);
                    if (closed.Contains(neighbor)) continue;
                    if (!IsEdgePassable(current, neighbor)) continue;

                    float tentativeG = gScore[current] + Mathf.Sqrt(dx * dx + dz * dz);
                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, endV);
                        open.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }
        }

        return null; // путь не найден
    }
}