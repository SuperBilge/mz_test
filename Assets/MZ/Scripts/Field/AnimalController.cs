using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MZ.Field
{
    public class AnimalController : FieldEntity
    {
        public bool canMoveDiagonal;
        [HideInInspector] public FeedController targetFeed;
        [HideInInspector] public int cachedPrioritySqrDistance;

        private static readonly Vector2Int[] DIRS_4 = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        private static readonly Vector2Int[] DIRS_8 = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        private HashSet<Vector2Int> _visited = new HashSet<Vector2Int>();
        private Queue<Vector2Int> _queue = new Queue<Vector2Int>();
        private Dictionary<Vector2Int, Vector2Int> _parent = new Dictionary<Vector2Int, Vector2Int>();
        private List<Vector2Int> _pathResult = new List<Vector2Int>();

        public List<Vector2Int> FindPath(Vector2Int target, int fieldLength, HashSet<Vector2Int> obstacles, bool canDiagonal)
        {
            _pathResult.Clear();

            if (position == target) return _pathResult;
            if (obstacles.Contains(target)) return null;

            var dirs = canDiagonal ? DIRS_8 : DIRS_4;

            _visited.Clear();
            _visited.Add(position);

            _queue.Clear();
            _queue.Enqueue(position);

            _parent.Clear();

            while (_queue.Count > 0)
            {
                var current = _queue.Dequeue();

                foreach (var dir in dirs)
                {
                    var next = new Vector2Int(current.x + dir.x, current.y + dir.y);

                    if (next == target)
                    {
                        _pathResult.Add(next);
                        var node = current;
                        while (node != position)
                        {
                            _pathResult.Add(node);
                            node = _parent[node];
                        }
                        _pathResult.Reverse();
                        return _pathResult;
                    }

                    if (next.x < 0 || next.x >= fieldLength || next.y < 0 || next.y >= fieldLength)
                        continue;
                    if (_visited.Contains(next))
                        continue;
                    if (obstacles.Contains(next))
                        continue;

                    _visited.Add(next);
                    _parent[next] = current;
                    _queue.Enqueue(next);
                }
            }

            return null;
        }

        public void MoveToCell(Vector2Int cell, float duration)
        {
            Vector3 targetPos = new Vector3(cell.x, 0, cell.y);
            position = cell;
            transform.DOKill();
            transform.DOLocalMove(targetPos, duration).SetEase(Ease.Linear);
        }
    }
}
