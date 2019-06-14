using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    /// <summary>
    /// Asynchronously generates Unity terrain objects for the positions requested,
    /// and stores them on an output queue.
    /// Terrain objects at neighboring positions should be contiguous.
    /// </summary>
    [System.Serializable]
    public class TerrainGenerator : MonoBehaviour
    {
        public class TerrainResult
        {
            public GameObject GameObject;
            public Vector2Int Position;

            public TerrainResult(GameObject gameObject, Vector2Int position)
            {
                GameObject = gameObject;
                Position = position;
            }
        }

        [Range(0, 4)]
        [Tooltip("Controls how much blending occurs in terrain texture splatting.")]
        public float TextureBlendSharpness = 1.0f;

        [Tooltip("Controls where each texture is present, and how prevalent relative to others.")]
        public TextureSplatSettings[] TextureSplatSettings;
        [Tooltip("Controls where trees are populated.")]
        public TreeInstanceSettings[] TreeInstanceSettings;
        [Tooltip("Controls where details like grass are populated.")]
        public DetailMapSettings[] DetailMapSettings;


        public TerrainNoise TerrainNoise;
        public Terrain TemplateTerrain;
        private TerrainData templateTerrainData;
        public ConcurrentQueue<TerrainResult> Output = new ConcurrentQueue<TerrainResult>();
        private ConcurrentQueue<Vector2Int> requests = new ConcurrentQueue<Vector2Int>();


        // Use this for initialization
        void Start()
        {
            if (TerrainNoise == null)
            {
                //TerrainNoise = GetComponent<TerrainNoise>();
                throw new System.NullReferenceException(
                    "No terrain noise component was assigned to the terrain generator in the unity editor.");
            }
            if (TemplateTerrain?.terrainData == null)
            {
                throw new System.NullReferenceException(
                    "No terrain template was assigned to the terrain generator in the unity editor.");
            }
            templateTerrainData = TemplateTerrain.terrainData;
        }

        public void Update()
        {
            // only start one per frame
            if (!requests.IsEmpty)
            {
                Vector2Int position = new Vector2Int();
                if (requests.TryDequeue(out position))
                {
                    StartCoroutine(TerrainCoroutine(position));
                }
            }
        }

        public void RequestTerrainTile(
            Vector2Int position)
        {
            requests.Enqueue(position);
        }

        private GameObject TerrainObject(TerrainData terrainData)
        {
            //TerrainData terrainData = await GenerateTerrainData(position, size, height, heightmapResolution, baseMapResolution, detailResolution);
            GameObject gameObject = Terrain.CreateTerrainGameObject(terrainData);
            Terrain terrain = gameObject.GetComponent<Terrain>();
            terrain.detailObjectDensity = TemplateTerrain.detailObjectDensity;
            terrain.detailObjectDistance = TemplateTerrain.detailObjectDistance;
            terrain.basemapDistance = TemplateTerrain.basemapDistance;
            terrain.heightmapPixelError = TemplateTerrain.heightmapPixelError;
            terrain.heightmapMaximumLOD = TemplateTerrain.heightmapMaximumLOD;
            terrain.treeBillboardDistance = TemplateTerrain.treeBillboardDistance;
            terrain.treeCrossFadeLength = TemplateTerrain.treeCrossFadeLength;
            terrain.treeDistance = TemplateTerrain.treeDistance;
            terrain.treeLODBiasMultiplier = TemplateTerrain.treeLODBiasMultiplier;
            terrain.treeMaximumFullLODCount = TemplateTerrain.treeMaximumFullLODCount;
            terrain.allowAutoConnect = TemplateTerrain.allowAutoConnect;
            terrain.drawInstanced = TemplateTerrain.drawInstanced;

            return gameObject;
        }


        IEnumerator TerrainCoroutine(
            Vector2Int position)
        {
            // Crete new TerrainData from the terrain template
            TerrainData terrainData = new TerrainData();
            TerrainUtilities.CopyTerrainData(templateTerrainData, terrainData);

            // Fill Heightmap
            int heightmapResolution = terrainData.heightmapResolution;
            float size = terrainData.size.x;
            float[,] heightMap = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
            Task heightMapTask = Task.Run(() => TerrainNoise.FillHeightMapNoise(
                (int)(position.x / size * (heightmapResolution - 1)),
                (int)(position.y / size * (heightmapResolution - 1)),
                heightMap));
            // Skip frames until heightmap is done
            while (!heightMapTask.IsCompleted)
            {
                yield return null;
            }
            //terrain.terrainData.SetHeightsDelayLOD(0, 0, heights);
            terrainData.SetHeights(0, 0, heightMap);

            // Create Detail Maps
            int detailResolution = terrainData.detailResolution;
            int numDetailLayers = terrainData.detailPrototypes.Length;
            int[][,] detailMaps = new int[numDetailLayers][,];
            Task[] tasks = new Task[numDetailLayers + 1];
            for (int i = 0; i < numDetailLayers; i++)
            {
                int[,] detailMap = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);
                detailMaps[i] = detailMap;
                tasks[i] = Task.Run(() => TerrainNoise.PopulateDetails(
                    (int)(position.x / size * detailResolution),
                    (int)(position.y / size * detailResolution),
                    ref detailMap, heightMap, i));
            }

            // Create terrain texture splat maps
            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            float terrainHeight = terrainData.size.y;
            float terrainWidth = terrainData.size.x;
            float terrainLength = terrainData.size.z;
            tasks[numDetailLayers] = Task.Run(() =>
                TerrainNoise.AddAlphaNoise(
                    TextureSplatSettings, alphaMaps, heightMap,
                    TerrainUtilities.HeightMapToSlopeMap(
                        heightMap,
                        terrainHeight,
                        terrainWidth,
                        terrainLength),
                    TextureBlendSharpness)
            );

            // Skip frames until detail and texture maps are done
            while (!Task.WhenAll(tasks).IsCompleted)
            {
                yield return null;
            }

            // Apply detail maps and texture maps to terrain
            terrainData.SetAlphamaps(0, 0, alphaMaps);
            yield return null;

            for (int i = 0; i < numDetailLayers; i++)
            {
                terrainData.SetDetailLayer(0, 0, i, detailMaps[i]);
                yield return null;
            }

            // create hidden terrain tile and add to the output queue
            GameObject obj = TerrainObject(terrainData);
            obj.SetActive(false);
            Output.Enqueue(new TerrainResult(obj, position));
        }
    }
}