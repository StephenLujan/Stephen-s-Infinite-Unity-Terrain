using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    public class TerrainManager : MonoBehaviour
    {
        //public TerrainGridSystem tgs;

        /// <summary>
        /// Terrain chunks that have have been deactivated because they are out of range
        /// </summary>
        ConcurrentDictionary<Vector2Int, GameObject> terrainCache = new ConcurrentDictionary<Vector2Int, GameObject>();

        /// <summary>
        /// Number of deactivated distant terrain chunks to retain in memory
        /// </summary>
        public int terrainCacheSize = 10;

        /// <summary>
        /// Stores all visible terrain chunks indexed by their position
        /// </summary>
        ConcurrentDictionary<Vector2Int, GameObject> terrainChunks = new ConcurrentDictionary<Vector2Int, GameObject>();

        /// <summary>
        /// chunks to deactivate
        /// </summary>
        ConcurrentQueue<GameObject> toDeactivate = new ConcurrentQueue<GameObject>();

        /// <summary>
        /// chunks to activate
        /// </summary>
        ConcurrentQueue<GameObject> toActivate = new ConcurrentQueue<GameObject>();

        /// <summary>
        /// Chunks to be Destroyed
        /// </summary>
        ConcurrentQueue<GameObject> toDestroy = new ConcurrentQueue<GameObject>();

        /// <summary>
        /// positions to create new terrain chunks
        /// </summary>
        ConcurrentQueue<Vector2Int> toCreate = new ConcurrentQueue<Vector2Int>();

        /// <summary>
        /// positions that have already be enqueued in the TerrainGenerator
        /// </summary>
        HashSet<Vector2Int> queuedGeneration = new HashSet<Vector2Int>();

        /// <summary>
        /// responsible for creating all new chunks of terrain, not positioning them or unloading them
        /// </summary>
        public TerrainGenerator TerrainGenerator;

        private int chunkSize;

        /// <summary>
        /// terrain chunks are created up to this radius in world units
        /// </summary>
        public int TerrainFillDistance = 2500;
        private int terrainFillDistanceSquared;

        /// <summary>
        /// the position of the viewer when the last chunk update occured
        /// </summary>
        protected Vector2 LastChunkUpdatePosition;

        // Use this for initialization
        void Start()
        {
            if (TerrainGenerator == null)
            {
                TerrainGenerator = GetComponent<TerrainGenerator>();
            }

            if (TerrainGenerator?.TemplateTerrain?.terrainData == null)
            {
                throw new System.NullReferenceException(
                    "No terrain template was assigned to the terrain generator in the unity editor.");
            }
            Terrain templateTerrain = TerrainGenerator.TemplateTerrain;
            chunkSize = (int)templateTerrain.terrainData.size.x;

            terrainFillDistanceSquared = TerrainFillDistance * TerrainFillDistance;
            float before = Time.realtimeSinceStartup;
            Debug.Log("Terrain Load");
            //tgs = TerrainGridSystem.instance;
            //tgs.GenerateMap();
            //tgs.cellBorderAlpha = 0.5f;

            float deltaTime = Time.realtimeSinceStartup - before;

            LastChunkUpdatePosition = Camera.main.transform.position;
            reenableOrGenerateCloseChunks(LastChunkUpdatePosition);

            //Debug.Log($"Finished Terrain Load in {deltaTime * 1000}ms");
        }

        // Update is called once per frame
        void Update()
        {
            // Start world update async tasks when camera moves a sufficient distance.
            // Results of the tasks will be picked up in later frames from the
            // queues toCreate, toDeactivate, etc.
            Vector2 viewerPosition = XZ(Camera.main.transform.position);
            if ((viewerPosition - LastChunkUpdatePosition).SqrMagnitude() >= chunkSize * chunkSize
                && toCreate.IsEmpty && queuedGeneration.Count == 0)
            {
                LastChunkUpdatePosition = viewerPosition;
                Task.Run(() =>
                {
                    turnOffDistantChunks(viewerPosition);
                    reenableOrGenerateCloseChunks(viewerPosition);
                    trimTerrainCache(viewerPosition);
                }).ConfigureAwait(false);
            }


            // send coordinates to the terrain generator
            if (toCreate.TryDequeue(out Vector2Int coordinates))
            {
                TerrainGenerator.EnqueueTerrain(coordinates);
                queuedGeneration.Add(coordinates);
            }

            // receive chunks from the terrain generator (several frames after they are enqueued)
            receiveGeneratedTerrain();

            // hide cached chunks unhide chunks moving from cached to visible
            GameObject chunk;
            while (toDeactivate.TryDequeue(out chunk))
            {
                chunk.SetActive(false);
            }
            while (toActivate.TryDequeue(out chunk))
            {
                chunk.SetActive(true);
            }
            while (toDestroy.TryDequeue(out chunk))
            {
                Destroy(chunk);
            }
        }

        /// <summary>
        /// receives finished terrain chunks from the TerrainGenerator and combines them with the overall terrain
        /// </summary>
        private void receiveGeneratedTerrain()
        {
            if (TerrainGenerator.Output.TryDequeue(out TerrainGenerator.TerrainResult terrainResult))
            {
                Vector2Int position = terrainResult.Position;
                GameObject obj = terrainResult.GameObject;
                obj.transform.SetParent(gameObject.transform);
                obj.transform.position = XZ(position);
                terrainChunks[position] = obj;
                setNeighbors(obj);
                obj.SetActive(true);
                queuedGeneration.Remove(position);
            }
        }

        /// <summary>
        /// removes the most distant terrainChunks from the cache until the cache is back within its size limit
        /// </summary>
        private void trimTerrainCache(Vector2 viewerPosition)
        {
            if (terrainCache.Count > terrainCacheSize)
            {
                while (terrainCache.Count > terrainCacheSize)
                {
                    Vector2Int furthest = terrainCache.Keys.First();
                    float furthestDistance = (furthest - viewerPosition).sqrMagnitude;
                    foreach (Vector2Int position in terrainCache.Keys)
                    {
                        float currentDistance = (position - viewerPosition).sqrMagnitude;
                        if (currentDistance > furthestDistance)
                        {
                            furthest = position;
                            furthestDistance = currentDistance;
                        }
                    }
                    terrainCache.TryRemove(furthest, out GameObject chunk);
                    toDestroy.Enqueue(chunk);
                }
            }
        }

        /// <summary>
        /// Removes visible terrain chunks and stores them in the cache
        /// </summary>
        private void turnOffDistantChunks(Vector2 viewerPosition)
        {
            foreach (KeyValuePair<Vector2Int, GameObject> kvp in terrainChunks)
            {
                Vector2Int position = kvp.Key;
                GameObject chunk = kvp.Value;
                if ((position - viewerPosition).sqrMagnitude > terrainFillDistanceSquared)
                {
                    toDeactivate.Enqueue(chunk);
                    terrainCache[position] = chunk;
                    terrainChunks.TryRemove(position, out chunk);
                }
            }
        }

        /// <summary>
        /// activates chunks from the cache or creates brand new chunks to fill the TerrainFillDistance radius
        /// </summary>
        private void reenableOrGenerateCloseChunks(Vector2 viewerPosition)
        {
            int chunkRadius = Mathf.CeilToInt(TerrainFillDistance / (float)chunkSize);
            int chunkCoordinateX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int chunkCoordinateY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
            Parallel.For(chunkCoordinateX - chunkRadius, chunkCoordinateX + chunkRadius + 1, (x) =>
            {
                x *= chunkSize;
                for (int y = chunkSize * (chunkCoordinateY - chunkRadius); y <= chunkSize * (chunkCoordinateY + chunkRadius); y += chunkSize)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    // if terrain chunk should be viewable
                    if ((position - viewerPosition).sqrMagnitude < terrainFillDistanceSquared)
                    {
                        if (terrainCache.ContainsKey(position))
                        {
                            GameObject chunk = terrainCache[position];
                            terrainChunks[position] = chunk;
                            terrainCache.TryRemove(position, out chunk);
                            toActivate.Enqueue(chunk);
                        }
                        else if (!terrainChunks.ContainsKey(position))
                        {
                            toCreate.Enqueue(position);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// avoids seams between terrain chunks by matching detail levels at the edges
        /// </summary>
        /// <param name="obj">a GameObject with a terrain component</param>
        /// <param name="steps">recursively calls this function on neighbors for this many steps (just use default value)</param>
        private void setNeighbors(GameObject obj, int steps = 2)
        {
            GameObject left = getNeighbor(obj, Direction.Left);
            GameObject top = getNeighbor(obj, Direction.Up);
            GameObject right = getNeighbor(obj, Direction.Right);
            GameObject bottom = getNeighbor(obj, Direction.Down);
            Terrain leftTerrain = left?.GetComponent<Terrain>();
            Terrain topTerrain = top?.GetComponent<Terrain>();
            Terrain rightTerrain = right?.GetComponent<Terrain>();
            Terrain bottomTerrain = bottom?.GetComponent<Terrain>();
            obj.GetComponent<Terrain>().SetNeighbors(leftTerrain, topTerrain, rightTerrain, bottomTerrain);
            steps--;
            if (steps > 0)
            {
                foreach (GameObject o in new GameObject[] { left, top, right, bottom })
                {
                    if (o != null)
                    {
                        setNeighbors(o, steps);
                    }
                }
            }
        }

        enum Direction { Up, Down, Left, Right }

        /// <summary>
        /// gets a terrain
        /// </summary>
        /// <param name="current">a GameObject with a terrain component</param>
        /// <param name="direction"></param>
        /// <returns>The neighbor in the specified direction or null if it does not exist</returns>
        private GameObject getNeighbor(GameObject current, Direction direction)
        {
            Vector2Int index = new Vector2Int((int)current.transform.position.x, (int)current.transform.position.z);
            switch (direction)
            {
                case (Direction.Up):
                    index.y += chunkSize;
                    break;
                case (Direction.Down):
                    index.y -= chunkSize;
                    break;
                case (Direction.Left):
                    index.x -= chunkSize;
                    break;
                case (Direction.Right):
                    index.x += chunkSize;
                    break;
            }
            if (terrainChunks.ContainsKey(index))
            {
                return terrainChunks[index];
            }
            return null;
        }

        public static Vector2 XZ(Vector3 input)
        {
            return new Vector2(input.x, input.z);
        }

        public static Vector3 XZ(Vector2 input)
        {
            return new Vector3(input.x, 0, input.y);
        }

        public static Vector3 XZ(Vector2Int input)
        {
            return new Vector3(input.x, 0, input.y);
        }

    }

}