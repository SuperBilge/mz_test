using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MZ.Field
{
    public class AnimalController : FieldEntity
    {
        public FeedController targetFeed;
        public bool canMoveDiagonal;

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

        public List<Vector2Int> FindPath(Vector2Int target, int fieldLength, HashSet<Vector2Int> obstacles, bool canDiagonal)
        {
            if (position == target) return new List<Vector2Int>();
            if (obstacles.Contains(target)) return null;

            var dirs = canDiagonal ? DIRS_8 : DIRS_4;
            var visited = new HashSet<Vector2Int> { position };
            var queue = new Queue<Vector2Int>();
            var parent = new Dictionary<Vector2Int, Vector2Int>();

            queue.Enqueue(position);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var dir in dirs)
                {
                    var next = new Vector2Int(current.x + dir.x, current.y + dir.y);

                    if (next == target)
                    {
                        var path = new List<Vector2Int> { next };
                        var node = current;
                        while (node != position)
                        {
                            path.Add(node);
                            node = parent[node];
                        }
                        path.Reverse();
                        return path;
                    }

                    if (next.x < 0 || next.x >= fieldLength || next.y < 0 || next.y >= fieldLength)
                        continue;
                    if (visited.Contains(next))
                        continue;
                    if (obstacles.Contains(next))
                        continue;

                    visited.Add(next);
                    parent[next] = current;
                    queue.Enqueue(next);
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
