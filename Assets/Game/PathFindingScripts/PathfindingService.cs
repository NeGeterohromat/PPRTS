using System;
using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using UnityEngine;

public class PathfindingService
{
    private const float RandomDiffRadius = 0.1f; // постройка отступает от края клетки на 0.1
    [DI] MouseSelector mouseSelector;

    //[DI] private BuildingData buildingData;

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
        int x1 = Mathf.Min(a.X, b.X);
        int z1 = Mathf.Min(a.Z, b.Z);

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
            var c1 = BuildingGridManager.Instance.GetDataInPosition(new Vector2Int(x1, z1));
            return !c1.isBuilding;
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


        if (dz == 0) // горизонтальное ребро
        {
            var lower = BuildingGridManager.Instance.GetDataInPosition(new Vector2Int(x1, z1 - 1));
            var upper = BuildingGridManager.Instance.GetDataInPosition(new Vector2Int(x1, z1));
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
            var left = BuildingGridManager.Instance.GetDataInPosition(new Vector2Int(x1 - 1, z1));
            var right = BuildingGridManager.Instance.GetDataInPosition(new Vector2Int(x1, z1));
            if (!left.isBuilding || !right.isBuilding) return true;
            return right.isLeftBorder || left.isRightBorder;
        }
    }

    // Эвристика — манхэттенское расстояние с диагоналями (можно и евклидово, но для сетки вершин лучше манхэттен)
    private float Heuristic(Vertex a, Vertex b)
    {
        int dx = Mathf.Abs(a.X - b.X);
        int dz = Mathf.Abs(a.Z - b.Z);

        int min = Mathf.Min(dx, dz);
        int max = Mathf.Max(dx, dz);

        return max + (Mathf.Sqrt(2f) - 1f) * min;
    }

    private IEnumerable<Func<int, IEnumerable<Vector2Int>>> FuncOrdder(Func<int, IEnumerable<Vector2Int>> f1, 
        Func<int, IEnumerable<Vector2Int>> f2, 
        Func<int, IEnumerable<Vector2Int>> f3, 
        Func<int, IEnumerable<Vector2Int>> f4,
        Func<int, IEnumerable<Vector2Int>> edge1,
        Func<int, IEnumerable<Vector2Int>> edge2,
        Func<int, IEnumerable<Vector2Int>> edge3,
        Func<int, IEnumerable<Vector2Int>> edge4,
        int loopCount)
    {
        for (int i = 0; i < loopCount; i++)
        {
            yield return f1;
            yield return edge1;
            yield return f2;
            yield return edge2;
            yield return f3;
            yield return edge3;
            yield return f4;
            yield return edge4;
        }
    }

    private IEnumerable<int> GetLoopOrder(int count)
    {
        yield return 0;
        for (int i = 0; i < count - 1; i++)
        {
            yield return (i / 2 + 1) * ((i % 2) * 2 - 1);
        }
    }

    private IEnumerable<Vector2Int> LoopLeftDirections(int radius)
    {
        foreach (var n in GetLoopOrder(radius*2-1))
        {
            yield return new Vector2Int(-radius, n);
        }
    }

    private IEnumerable<Vector2Int> LoopRightDirections(int radius)
    {
        foreach (var n in GetLoopOrder(radius * 2 - 1))
        {
            yield return new Vector2Int(radius, n);
        }
    }

    private IEnumerable<Vector2Int> LoopDownDirections(int radius)
    {
        foreach (var n in GetLoopOrder(radius * 2 - 1))
        {
            yield return new Vector2Int(n, -radius);
        }
    }

    private IEnumerable<Vector2Int> LoopUpDirections(int radius)
    {
        foreach (var n in GetLoopOrder(radius * 2 - 1))
        {
            yield return new Vector2Int(n, radius);
        }
    }

    private IEnumerable<Vector2Int> GetUpRightEdge(int radius)
    {
        yield return new Vector2Int(radius, radius);
    }

    private IEnumerable<Vector2Int> GetUpLeftEdge(int radius)
    {
        yield return new Vector2Int(-radius, radius);
    }

    private IEnumerable<Vector2Int> GetDownRightEdge(int radius)
    {
        yield return new Vector2Int(radius, -radius);
    }

    private IEnumerable<Vector2Int> GetDownLeftEdge(int radius)
    {
        yield return new Vector2Int(-radius, -radius);
    }

    private IEnumerable<Vector2Int> LoopDirections(Vector2 cell, int radiusCount)
    {
        var order = FuncOrdder(LoopUpDirections, LoopRightDirections, LoopDownDirections, LoopLeftDirections, 
            GetUpRightEdge, GetDownRightEdge, GetDownLeftEdge, GetUpLeftEdge,
            radiusCount);
        if (cell.x - Mathf.Floor(cell.x) < 0.5f && Mathf.Ceil(cell.y) - cell.y < 0.5f)
            order = FuncOrdder(LoopUpDirections, LoopLeftDirections, LoopDownDirections, LoopRightDirections,
                GetUpLeftEdge, GetDownLeftEdge, GetDownRightEdge, GetUpRightEdge,
                radiusCount);
        if (cell.x - Mathf.Floor(cell.x) < 0.5f && cell.y - Mathf.Floor(cell.y) < 0.5f)
            order = FuncOrdder(LoopDownDirections, LoopLeftDirections, LoopUpDirections, LoopRightDirections,
                GetDownLeftEdge, GetUpLeftEdge, GetUpRightEdge, GetDownRightEdge,
                radiusCount);
        if (cell.y - Mathf.Floor(cell.y) < 0.5f && cell.x - Mathf.Ceil(cell.x) < 0.5f)
            order = FuncOrdder(LoopDownDirections, LoopRightDirections, LoopUpDirections, LoopLeftDirections,
                GetDownRightEdge, GetUpRightEdge, GetUpLeftEdge, GetDownLeftEdge,
                radiusCount);

        int radius = 1;
        int counter = 0;
        foreach (var directionLooper in order)
        {
            foreach(var direction in directionLooper(radius))
            {
                yield return direction;
            }

            counter = (counter + 1) % 8;
            if (counter == 0)
                radius++;
        }
    }

    // Поиск ближайшей свободной точки, если цель внутри постройки
    public Vector2 ResolveGoal(Vector2 goal)
    {
        var CellLength = mouseSelector.GridSize;
        // небольшой разброс, чтобы все юниты не отправлялись в одну точку
        var randomShift = UnityEngine.Random.insideUnitCircle * RandomDiffRadius;        

        var cellWithoutRound = new Vector2(goal.x / CellLength, goal.y / CellLength);
        var cell = new Vector2Int(Mathf.FloorToInt(goal.x / CellLength), Mathf.FloorToInt(goal.y / CellLength));
        var data = BuildingGridManager.Instance.GetDataInPosition(cell);
        if (!data.isBuilding) return goal + randomShift;

        var res = goal;

        // Ищем ближайшую свободную клетку
        foreach (var dir in LoopDirections(cellWithoutRound, 5))
        {
            var neighbor = cell + dir;
            Debug.Log($"dir {dir} {neighbor}");
            var nd = BuildingGridManager.Instance.GetDataInPosition(neighbor);

            if (Mathf.Abs(dir.x) == Mathf.Abs(dir.y))
            {
                if (!nd.isBuilding)
                {
                    // Возвращаем левый нижний угол клетки neighbour, если neighbour это правый верхний сосед
                    int dx = 1;
                    int dy = 1;
                    if (dir.x > 0 && dir.y < 0)
                        dx = 0;
                    if (dir.x < 0 && dir.y > 0)
                        dy = 0;
                    if (dir.x < 0 && dir.y < 0)
                    {
                        dx = 0;
                        dy = 0;
                    }
                    res = (neighbor + new Vector2(dx,dy)) * CellLength;
                    Debug.Log($"res {res} , {dir * (new Vector2(-1f / Mathf.Abs(dir.x), -1f / Mathf.Abs(dir.y)))} , {neighbor + (dir * (new Vector2(-1f / dir.x, -1f / dir.y)))}");
                    break;
                }
            }
            else
            {
                if (dir.y < 0 && (!nd.isBuilding || nd.isUpBorder)) 
                {
                    var x = goal.x;
                    if (dir.x < 0) x = (neighbor.x + 1) * CellLength;
                    if (dir.x > 0) x = neighbor.x * CellLength;
                    res = new Vector2(x, (neighbor.y + 1) * CellLength);
                    break;
                }
                if (dir.y > 0 && (!nd.isBuilding || nd.isDownBorder))
                {
                    var x = goal.x;
                    if (dir.x < 0) x = (neighbor.x + 1) * CellLength;
                    if (dir.x > 0) x = neighbor.x * CellLength;
                    res = new Vector2(x, neighbor.y * CellLength);
                    break;
                }
                if (dir.x < 0 && (!nd.isBuilding || nd.isRightBorder))
                {
                    var y = goal.y;
                    if (dir.y < 0) y = (neighbor.y + 1) * CellLength;
                    if (dir.y > 0) y = neighbor.y * CellLength;
                    res = new Vector2((neighbor.x + 1) * CellLength, y);
                    break;
                }
                if (dir.x > 0 && (!nd.isBuilding || nd.isLeftBorder))
                {
                    var y = goal.y;
                    if (dir.y < 0) y = (neighbor.y + 1) * CellLength;
                    if (dir.y > 0) y = neighbor.y * CellLength;
                    res = new Vector2(neighbor.x * CellLength, y);
                    break;
                }
            }
        }
        return res + randomShift; // fallback
    }

    // Основной поиск пути между двумя мировыми точками
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        var CellLength = mouseSelector.GridSize;
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
                var path = new List<Vector2>() { end };
                var lastNode = current;

                Vertex node;
                if (cameFrom.ContainsKey(lastNode))
                {
                    node = cameFrom[lastNode];
                    while (IsNodeInOneCellWithVector(end, node))
                    {                        
                        lastNode = node;
                        if (!cameFrom.ContainsKey(node)) { path.Add(new Vector2(node.X * CellLength, node.Z * CellLength)); break; }
                        node = cameFrom[node];
                    }
                }
                node = lastNode;
                while (cameFrom.ContainsKey(node))
                {
                    path.Add(new Vector2(node.X * CellLength, node.Z * CellLength));
                    lastNode = node;
                    node = cameFrom[node];
                    if (IsNodeInOneCellWithVector(start, lastNode)) break;
                }
                var b = IsNodeInOneCellWithVector(start, lastNode);
                Debug.Log($"{b} {start} {lastNode.X} {lastNode.Z}");
                if (!b)
                    path.Add(new Vector2(startV.X * CellLength, startV.Z * CellLength));
                path.Reverse();
                Debug.Log(string.Join(' ',path));
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

    private bool IsNodeInOneCellWithVector(Vector2 vec, Vertex node)
    {
        var CellLength = mouseSelector.GridSize;
        var cell = new Vector2Int(Mathf.FloorToInt(vec.x / CellLength), Mathf.FloorToInt(vec.y / CellLength));
        // Если вектор попал внутрь здания, мы не будем пропусать ни один ход.
        if (BuildingGridManager.Instance.GetDataInPosition(cell).isBuilding) return false;

        var v1 = new Vertex(Mathf.CeilToInt(vec.x / CellLength), Mathf.CeilToInt(vec.y / CellLength));
        var v2 = new Vertex(Mathf.FloorToInt(vec.x / CellLength), Mathf.CeilToInt(vec.y / CellLength));
        var v3 = new Vertex(Mathf.CeilToInt(vec.x / CellLength), Mathf.FloorToInt(vec.y / CellLength));
        var v4 = new Vertex(Mathf.FloorToInt(vec.x / CellLength), Mathf.FloorToInt(vec.y / CellLength));
        return node.Equals(v1) || node.Equals(v2) || node.Equals(v3) || node.Equals(v4);
    }
}