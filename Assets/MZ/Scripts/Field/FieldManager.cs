using System.Collections.Generic;
using MZ.Sim;
using UnityEngine;

namespace MZ.Field
{
    public class FieldManager : MonoBehaviour
    {
        [SerializeField] private Transform tilesParent;
        [SerializeField] private Transform animalsParent;
        [SerializeField] private Transform feedParent;
        [SerializeField] private Color[] tileColors;
        [SerializeField] private FieldTileController fieldTilePrefab;
        [SerializeField] private AnimalController animalPrefab;
        [SerializeField] private FeedController feedPrefab;

        private FieldTileController[] _fieldTiles;
        private List<AnimalController> _animals;
        private List<FeedController> _feeds;

        public void InitField(SimParams simParams)
        {
            int index = 0;
            int fieldLength = simParams.FieldLength;

            _fieldTiles = new FieldTileController[fieldLength * fieldLength];

            for (int x = 0; x < fieldLength; x++)
            {
                for (int y = 0; y < fieldLength; y++)
                {
                    FieldTileController tile = CreateTile(index, x, y);
                    _fieldTiles[index] = tile;
                    index++;
                }
            }

            _animals = new List<AnimalController>();
            _feeds = new List<FeedController>();

            if (simParams.Animals != null && simParams.Animals.Length > 0)
            {
                RestoreAnimalsAndFeed(simParams);
            }
            else
            {
                CreateAnimalsAndFeed(simParams);
            }
        }

        private void RestoreAnimalsAndFeed(SimParams simParams)
        {
            int index = 0;
            foreach (var state in simParams.Animals)
            {
                Color color = new Color(state.ColorR, state.ColorG, state.ColorB, 1f);
                AnimalController animal = CreateAnimal(index, state.PositionX, state.PositionY, color);
                _animals.Add(animal);
                index++;
            }

            index = 0;
            foreach (var state in simParams.Feeds)
            {
                Color color = new Color(state.ColorR, state.ColorG, state.ColorB, 1f);
                FeedController feed = CreateFeed(index, state.PositionX, state.PositionY, color);
                _feeds.Add(feed);
                index++;
            }
        }

        private void CreateAnimalsAndFeed(SimParams simParams)
        {
            int fieldLength = simParams.FieldLength;
            int animalsCount = simParams.AnimalsCount;
            float maxFeedDistance = simParams.AnimalSpeed * 5f;

            HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

            for (int i = 0; i < animalsCount; i++)
            {
                Vector2Int animalPos = FindRandomFreeCell(fieldLength, occupiedCells);
                if (animalPos.x < 0) break;

                occupiedCells.Add(animalPos);

                Color color = GetRandomColor();
                AnimalController animal = CreateAnimal(i, animalPos.x, animalPos.y, color);
                _animals.Add(animal);

                Vector2Int feedPos = FindFeedPosition(animalPos, fieldLength, maxFeedDistance, occupiedCells);
                FeedController feed = CreateFeed(i, feedPos.x, feedPos.y, color);
                _feeds.Add(feed);
            }
        }

        private Vector2Int FindRandomFreeCell(int fieldLength, HashSet<Vector2Int> occupied)
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int x = Random.Range(0, fieldLength);
                int y = Random.Range(0, fieldLength);
                Vector2Int pos = new Vector2Int(x, y);
                if (!occupied.Contains(pos))
                    return pos;
            }
            return new Vector2Int(-1, -1);
        }

        private Vector2Int FindFeedPosition(Vector2Int animalPos, int fieldLength, float maxDistance, HashSet<Vector2Int> occupiedCells)
        {
            int maxDistInt = Mathf.CeilToInt(maxDistance);
            int minX = Mathf.Max(0, animalPos.x - maxDistInt);
            int maxX = Mathf.Min(fieldLength - 1, animalPos.x + maxDistInt);
            int minY = Mathf.Max(0, animalPos.y - maxDistInt);
            int maxY = Mathf.Min(fieldLength - 1, animalPos.y + maxDistInt);

            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Random.Range(minX, maxX + 1);
                int y = Random.Range(minY, maxY + 1);
                Vector2Int pos = new Vector2Int(x, y);
                float dist = Vector2Int.Distance(pos, animalPos);
                if (dist <= maxDistance && dist > 0f && !occupiedCells.Contains(pos))
                    return pos;
            }

            Vector2Int fallback = new Vector2Int(
                Mathf.Min(animalPos.x + 1, fieldLength - 1),
                animalPos.y
            );
            return occupiedCells.Contains(fallback) ? animalPos : fallback;
        }

        private AnimalController CreateAnimal(int i, int x, int y, Color color)
        {
            AnimalController animal = Instantiate(animalPrefab, animalsParent);
            animal.Init(i, x, y, color);
            return animal;
        }

        private FeedController CreateFeed(int i, int x, int y, Color color)
        {
            FeedController feed = Instantiate(feedPrefab, feedParent);
            feed.Init(i, x, y, color);
            return feed;
        }

        private FieldTileController CreateTile(int i, int x, int y)
        {
            FieldTileController ftc = Instantiate(fieldTilePrefab, tilesParent);
            Color clr = GetTileColor(x + y);
            ftc.Init(i, x, y, clr);
            return ftc;
        }

        private Color GetTileColor(int i)
        {
            return i % 2 == 0 ? tileColors[0] : tileColors[1];
        }

        private Color GetRandomColor()
        {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }

        public void GetState(out EntityState[] animals, out EntityState[] feeds)
        {
            if (_animals == null)
            {
                animals = new EntityState[0];
                feeds = new EntityState[0];
                return;
            }

            animals = new EntityState[_animals.Count];
            for (int i = 0; i < _animals.Count; i++)
            {
                var a = _animals[i];
                animals[i] = new EntityState
                {
                    Id = a.id,
                    PositionX = a.position.x,
                    PositionY = a.position.y,
                    ColorR = a.EntityColor.r,
                    ColorG = a.EntityColor.g,
                    ColorB = a.EntityColor.b
                };
            }

            feeds = new EntityState[_feeds.Count];
            for (int i = 0; i < _feeds.Count; i++)
            {
                var f = _feeds[i];
                feeds[i] = new EntityState
                {
                    Id = f.id,
                    PositionX = f.position.x,
                    PositionY = f.position.y,
                    ColorR = f.EntityColor.r,
                    ColorG = f.EntityColor.g,
                    ColorB = f.EntityColor.b
                };
            }
        }
    }
}
