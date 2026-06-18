using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MZ.Sim;
using MZ.Utility;
using UnityEngine;
using UnityEngine.Pool;

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
        [SerializeField] private FeedParticle particlePrefab;
        [SerializeField] private float initialTickDuration = 1f;

        private FieldTileController[] _fieldTiles;
        private List<AnimalController> _animals;
        private List<FeedController> _feeds;
        private ObjectPool<FeedParticle> _particlePool;
        private Coroutine _simCoroutine;
        private int _currentFieldLength;
        private int _currentAnimalSpeed;
        private int _simSpeed;

        public void SetSimSpeed(int speedValue)
        {
            _simSpeed = speedValue;
        }

        public void InitField(SimParams simParams)
        {
            ClearField();

            int index = 0;
            int fieldLength = simParams.FieldLength;

            _currentFieldLength = fieldLength;
            _currentAnimalSpeed = simParams.AnimalSpeed;

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

            LinkAnimalsAndFeed();

            _particlePool = new ObjectPool<FeedParticle>(
                createFunc: () =>
                {
                    var p = Instantiate(particlePrefab, transform);
                    p.Init(particle => _particlePool.Release(particle));
                    p.gameObject.SetActive(false);
                    return p;
                },
                actionOnGet: p => p.gameObject.SetActive(true),
                actionOnRelease: p => p.gameObject.SetActive(false),
                actionOnDestroy: p => Destroy(p.gameObject),
                maxSize: 20
            );

            _simCoroutine = StartCoroutine(SimLoop());
        }

        private void ClearField()
        {
            if (_simCoroutine != null)
            {
                StopCoroutine(_simCoroutine);
                _simCoroutine = null;
            }

            ClearChildren(tilesParent);
            ClearChildren(animalsParent);
            ClearChildren(feedParent);

            var particles = GetComponentsInChildren<FeedParticle>(true);
            foreach (var p in particles)
                Destroy(p.gameObject);

            _fieldTiles = null;
            _animals = null;
            _feeds = null;
            _particlePool = null;
        }

        private void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private void LinkAnimalsAndFeed()
        {
            for (int i = 0; i < _animals.Count && i < _feeds.Count; i++)
            {
                _animals[i].targetFeed = _feeds[i];
                _feeds[i].ownerAnimal = _animals[i];
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

        private IEnumerator SimLoop()
        {
            while (true)
            {
                if (_simSpeed <= 0 || _animals == null || _animals.Count == 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                yield return SimTick();
            }
        }

        private IEnumerator SimTick()
        {
            float tickDuration = initialTickDuration / _currentAnimalSpeed / _simSpeed;

            var activeAnimals = new List<AnimalController>();
            foreach (var a in _animals)
            {
                if (a.position != a.targetFeed.position)
                    activeAnimals.Add(a);
            }

            if (activeAnimals.Count == 0)
            {
                yield return new WaitForSeconds(tickDuration);
                yield break;
            }

            activeAnimals.Sort((a, b) =>
            {
                float distA = Vector2Int.Distance(a.position, a.targetFeed.position);
                float distB = Vector2Int.Distance(b.position, b.targetFeed.position);
                int cmp = distA.CompareTo(distB);
                if (cmp != 0) return cmp;
                return a.id.CompareTo(b.id);
            });

            var obstacles = new HashSet<Vector2Int>();
            foreach (var a in _animals)
                obstacles.Add(a.position);

            foreach (var animal in activeAnimals)
            {
                var blocked = new HashSet<Vector2Int>(obstacles);
                blocked.Remove(animal.position);

                var path = animal.FindPath(animal.targetFeed.position, _currentFieldLength, blocked, animal.canMoveDiagonal);

                if (path != null && path.Count > 0)
                {
                    Vector2Int nextCell = path[0];
                    obstacles.Add(nextCell);
                    animal.MoveToCell(nextCell, tickDuration);
                }
            }

            yield return new WaitForSeconds(tickDuration);

            for (int i = _animals.Count - 1; i >= 0; i--)
            {
                var animal = _animals[i];
                if (animal.position == animal.targetFeed.position)
                {
                    HandleAnimalEats(animal);
                }
            }
        }

        private void HandleAnimalEats(AnimalController animal)
        {
            var feed = animal.targetFeed;
            var feedColor = feed.EntityColor;
            Vector3 feedWorldPos = feed.transform.position;

            var particle = _particlePool.Get();
            if (particle != null)
            {
                particle.transform.position = feedWorldPos;
                particle.Show(feedColor);
            }

            var occupiedCells = new HashSet<Vector2Int>();
            foreach (var a in _animals)
                occupiedCells.Add(a.position);

            float maxDist = _currentAnimalSpeed * 5f;
            feed.Respawn(_currentFieldLength, maxDist, occupiedCells);
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
