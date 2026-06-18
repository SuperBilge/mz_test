using System.Collections.Generic;
using UnityEngine;

namespace MZ.Field
{
    public class FeedController : FieldEntity
    {
        [HideInInspector] public AnimalController ownerAnimal;

        public void Respawn(int fieldLength, float maxDistance, HashSet<Vector2Int> occupiedCells)
        {
            int maxDistInt = Mathf.CeilToInt(maxDistance);
            int minX = Mathf.Max(0, ownerAnimal.position.x - maxDistInt);
            int maxX = Mathf.Min(fieldLength - 1, ownerAnimal.position.x + maxDistInt);
            int minY = Mathf.Max(0, ownerAnimal.position.y - maxDistInt);
            int maxY = Mathf.Min(fieldLength - 1, ownerAnimal.position.y + maxDistInt);

            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Random.Range(minX, maxX + 1);
                int y = Random.Range(minY, maxY + 1);
                Vector2Int newPos = new Vector2Int(x, y);
                float dist = Vector2Int.Distance(newPos, ownerAnimal.position);

                if (dist <= maxDistance && dist > 0f && !occupiedCells.Contains(newPos))
                {
                    position = newPos;
                    transform.localPosition = new Vector3(newPos.x, 0, newPos.y);
                    return;
                }
            }

            Vector2Int fallback = new Vector2Int(
                Mathf.Min(ownerAnimal.position.x + 1, fieldLength - 1),
                ownerAnimal.position.y
            );
            position = fallback;
            transform.localPosition = new Vector3(fallback.x, 0, fallback.y);
        }
    }
}
