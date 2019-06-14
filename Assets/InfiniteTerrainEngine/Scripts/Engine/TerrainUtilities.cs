
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    public static class TerrainUtilities
    {
        /// <summary>
        /// y,x indexed like heightmaps
        /// </summary>
        /// <param name="heightMap"></param>
        /// <param name="terrainHeight"></param>
        /// <param name="terrainWidth"></param>
        /// <param name="terrainLength"></param>
        /// <returns>y,x indexed like heightmaps values from 0 (flat) to 1 (vertical)</returns>
        public static float[,] HeightMapToSlopeMap(float[,] heightMap, float terrainHeight, float terrainWidth, float terrainLength)
        {
            const float halfpi = Mathf.PI / 2.0f;
            float heightToWidth = terrainHeight / terrainWidth;
            float heightToLength = terrainHeight / terrainLength;

            int heightMapWidth = heightMap.GetUpperBound(1) + 1;
            int heightMapHeight = heightMap.GetUpperBound(0) + 1;

            float[,] slopeMap = new float[heightMapHeight, heightMapWidth];

            for (int y = 0; y < heightMapHeight; y++)
            {
                for (int x = 0; x < heightMapWidth; x++)
                {
                    // use self instead of neighbors where a neighbor is unavailable at edges
                    int xPlus1 = (x == heightMapWidth - 1) ? x : x + 1;
                    int xMinus1 = (x == 0) ? x : x - 1;

                    int yPlus1 = (y == heightMapHeight - 1) ? y : y + 1;
                    int yMinus1 = (y == 0) ? y : y - 1;

                    // get heights of neighbors
                    float left = heightMap[y, xMinus1];
                    float right = heightMap[y, xPlus1];

                    float down = heightMap[yMinus1, x];
                    float up = heightMap[yPlus1, x];

                    float xHeightChange = (right - left) * heightToWidth / 2.0f;
                    float yHeightChange = (up - down) * heightToLength / 2.0f;
                    float slope = Mathf.Sqrt(xHeightChange * xHeightChange + yHeightChange * yHeightChange);
                    slopeMap[y, x] = Mathf.Atan(slope) / halfpi;
                }
            }
            return slopeMap;
        }

        public static void CopyTerrainData(TerrainData source, TerrainData destination)
        {
            //destination.alphamapResolution = source.alphamapResolution;
            //destination.detailPrototypes = source.detailPrototypes;
            //destination.treePrototypes = source.treePrototypes;
            //destination.terrainLayers = source.terrainLayers;
            //destination.heightmapResolution = source.heightmapResolution;
            //destination.thickness = source.thickness;
            //destination.wavingGrassAmount = source.wavingGrassAmount;
            //destination.wavingGrassSpeed = source.wavingGrassSpeed;
            //destination.wavingGrassStrength = source.wavingGrassStrength;
            //destination.wavingGrassTint = source.wavingGrassTint;
            //destination.baseMapResolution = source.baseMapResolution;
            //destination.size = source.size;
            CopyFrom(destination, source);

            destination.SetDetailResolution(source.detailResolution, source.detailResolutionPerPatch);
            destination.SetAlphamaps(0, 0, source.GetAlphamaps(0, 0, source.alphamapWidth, source.alphamapHeight));
        }

        public static void CopyTerrain(Terrain source, Terrain destination)
        {
            //destination.detailObjectDensity = source.detailObjectDensity;
            //destination.detailObjectDistance = source.detailObjectDistance;
            //destination.basemapDistance = source.basemapDistance;
            //destination.heightmapPixelError = source.heightmapPixelError;
            //destination.heightmapMaximumLOD = source.heightmapMaximumLOD;
            //destination.treeBillboardDistance = source.treeBillboardDistance;
            //destination.treeCrossFadeLength = source.treeCrossFadeLength;
            //destination.treeDistance = source.treeDistance;
            //destination.treeLODBiasMultiplier = source.treeLODBiasMultiplier;
            //destination.treeMaximumFullLODCount = source.treeMaximumFullLODCount;
            //destination.allowAutoConnect = source.allowAutoConnect;
            //destination.drawInstanced = source.drawInstanced;
            //destination.bakeLightProbesForTrees = source.bakeLightProbesForTrees;
            //destination.collectDetailPatches = source.collectDetailPatches;
            //destination.deringLightProbesForTrees = source.deringLightProbesForTrees;
            //destination.drawHeightmap = source.drawHeightmap;
            //destination.drawTreesAndFoliage = source.drawTreesAndFoliage;
            //destination.editorRenderFlags = source.editorRenderFlags;
            //destination.freeUnusedRenderingResources = source.freeUnusedRenderingResources;
            //destination.hideFlags = source.hideFlags;

            CopyFrom(destination, source, "terrainData");
        }

        public static void CopyFrom<T1, T2>(T1 obj, T2 otherObject, params string[] excluding)
            where T1 : class
            where T2 : class
        {
            PropertyInfo[] srcFields = otherObject.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            if (excluding.Length > 0)
            {
                srcFields = srcFields.Where(x => !excluding.Contains(x.Name)).ToArray();
            }

            PropertyInfo[] destFields = obj.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

            foreach (PropertyInfo property in srcFields)
            {
                PropertyInfo dest = destFields.FirstOrDefault(x => x.Name == property.Name);
                if (dest != null && dest.CanWrite)
                {
                    dest.SetValue(obj, property.GetValue(otherObject, null), null);
                }
            }
        }

        public static T Measure<T>(Func<T> func, params object[] args)
        {
            float TicksPerMillisecond = Stopwatch.Frequency / 1000.0f;
            Stopwatch sw = Stopwatch.StartNew();
            T output = (T)func.DynamicInvoke(args);
            UnityEngine.Debug.Log($"{func.Method.Name} took {sw.ElapsedTicks / TicksPerMillisecond}ms");
            return output;
        }

        public static void Measure<T>(Action<T> action, params object[] args)
        {
            float TicksPerMillisecond = Stopwatch.Frequency / 1000.0f;
            Stopwatch sw = Stopwatch.StartNew();
            action.DynamicInvoke(args);
            UnityEngine.Debug.Log($"{action.Method.Name} took {sw.ElapsedTicks / TicksPerMillisecond}ms");
        }
    }
}
