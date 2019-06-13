using NUnit.Framework;
using StephenLujan.TerrainEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Assets.InfiniteTerrainEngine.Scripts.Tests
{
    using NoiseSource = Func<GameObject, UnityNoiseBase>;

    class TerrainNoiseTests
    {
        public static UnityNoiseBase addUnityNoise(GameObject gameObject)
        {
            return gameObject.AddComponent<UnityNoise>();
        }

        public static UnityNoiseBase addFastNoiseSIMDUnity(GameObject gameObject)
        {
            FastNoiseSIMDUnity fastNoise = gameObject.AddComponent<FastNoiseSIMDUnity>();
            FastNoiseSIMDUnityWrapper wrapper = gameObject.AddComponent<FastNoiseSIMDUnityWrapper>();
            wrapper.fastNoiseSIMDUnity = fastNoise;
            return wrapper;
        }

        public static UnityNoiseBase addFastNoiseUnity(GameObject gameObject)
        {
            FastNoiseUnity fastNoise = gameObject.AddComponent<FastNoiseUnity>();
            FastNoiseUnityWrapper wrapper = gameObject.AddComponent<FastNoiseUnityWrapper>();
            wrapper.fastNoiseUnity = fastNoise;
            return wrapper;
        }


        public static TerrainNoise makeTerrainNoise(NoiseSource noiseSource)
        {
            GameObject gameObject = new GameObject();
            UnityNoiseBase unityNoise = noiseSource(gameObject);
            TerrainNoise terrainNoise = gameObject.AddComponent<TerrainNoise>();
            AnimationCurve animationCurve = new AnimationCurve()
            {
                keys = new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) }
            };
            terrainNoise.HeightNoise = unityNoise;
            terrainNoise.DetailNoise = unityNoise;
            terrainNoise.HeightAdjustCurve = animationCurve;
            //unityNoise.Awake();
            terrainNoise.Start();
            return terrainNoise;
        }

        [UnityTest]
        public IEnumerator TerrainNoiseOutputsNotFlat_FastNoiseSIMD()
        {
            return TerrainNoiseOutputsNotFlat(addFastNoiseSIMDUnity);
        }

        [UnityTest]
        public IEnumerator TerrainNoiseOutputsNotFlat_FastNoise()
        {
            return TerrainNoiseOutputsNotFlat(addFastNoiseUnity);
        }

        [UnityTest]
        public IEnumerator TerrainNoiseOutputsNotFlat_MyNoise()
        {
            return TerrainNoiseOutputsNotFlat(addUnityNoise);
        }

        [UnityTest]
        public IEnumerator NoiseOutputsConnect_FastNoiseSIMD()
        {
            return NoiseOutputsConnect(addFastNoiseSIMDUnity);
        }

        [UnityTest]
        public IEnumerator NoiseOutputsConnect_FastNoise()
        {
            return NoiseOutputsConnect(addFastNoiseUnity);
        }

        [UnityTest]
        public IEnumerator NoiseOutputsConnect_MyNoise()
        {
            return NoiseOutputsConnect(addUnityNoise);
        }

        public IEnumerator TerrainNoiseOutputsNotFlat(NoiseSource noiseSource)
        {
            TerrainNoise terrainNoise = makeTerrainNoise(noiseSource);
            float[,] heightMap = new float[3, 3];
            terrainNoise.FillHeightMapNoise(0, 0, heightMap);
            Assert.AreNotEqual(heightMap[0, 0], heightMap[0, 2]);
            Assert.AreNotEqual(heightMap[0, 0], heightMap[2, 0]);
            yield return null;
        }


        public IEnumerator NoiseOutputsConnect(NoiseSource noiseSource)
        {
            // NOTE: Unity heightmaps are y,x indexed, even though it is inconsistent with everything else.

            TerrainNoise terrainNoise = makeTerrainNoise(noiseSource);

            float[,] middleMap = new float[3, 3];
            float[,] rightMap = new float[3, 3];
            float[,] upMap = new float[3, 3];
            float[,] leftMap = new float[3, 3];
            float[,] downMap = new float[3, 3];

            terrainNoise.FillHeightMapNoise(0, 0, middleMap);
            terrainNoise.FillHeightMapNoise(2, 0, rightMap);
            terrainNoise.FillHeightMapNoise(0, 2, upMap);
            terrainNoise.FillHeightMapNoise(-2, 0, leftMap);
            terrainNoise.FillHeightMapNoise(0, -2, downMap);

            foreach (KeyValuePair<string, float[,]> pair in new Dictionary<string, float[,]>() {
                { "rightMap", rightMap},
                { "upMap", upMap},
                { "leftMap", leftMap},
                { "downMap", downMap}
            })
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x2 = 0; x2 < 3; x2++)
                        {
                            for (int y2 = 0; y2 < 3; y2++)
                            {
                                if (middleMap[y, x] == pair.Value[y, x])
                                {
                                    Debug.Log($"MiddleMap coordinates {y},{x} matches {pair.Key} {y2},{x2}");
                                }
                            }
                        }
                    }
                }
                Debug.Log(pair.Key);
            }

            Assert.AreEqual(middleMap[0, 2], rightMap[0, 0]);
            Assert.AreEqual(middleMap[1, 2], rightMap[1, 0]);
            Assert.AreEqual(middleMap[2, 2], rightMap[2, 0]);

            Assert.AreEqual(middleMap[2, 0], upMap[0, 0]);
            Assert.AreEqual(middleMap[2, 1], upMap[0, 1]);
            Assert.AreEqual(middleMap[2, 2], upMap[0, 2]);

            Assert.AreEqual(middleMap[0, 0], leftMap[0, 2]);
            Assert.AreEqual(middleMap[1, 0], leftMap[1, 2]);
            Assert.AreEqual(middleMap[2, 0], leftMap[2, 2]);

            Assert.AreEqual(middleMap[0, 0], downMap[2, 0]);
            Assert.AreEqual(middleMap[0, 1], downMap[2, 1]);
            Assert.AreEqual(middleMap[0, 2], downMap[2, 2]);

            yield return null;
        }
    }
}
