using NUnit.Framework;
using StephenLujan.TerrainEngine;
using System;
using UnityEngine;

namespace Assets.InfiniteTerrainEngine.Scripts.Tests
{
    class TerrainUtilitiesTests
    {
        [Test]
        public void TerrainSlopeTest()
        {
            float[,] heightmapX = new float[,] { { 0, 0, 0 }, { 1, 1, 1 }, { 2, 2, 2 } };
            float[,] heightmapY = new float[,] { { 0, 1, 2 }, { 0, 1, 2 }, { 0, 1, 2 } };
            float[,] heightmapXY = new float[,] { { 0, 1, 2 }, { 1, 2, 3 }, { 2, 3, 4 } };

            float[,] slopemap = TerrainUtilities.HeightMapToSlopeMap(heightmapX, 1, 1, 1);
            Console.WriteLine($"{slopemap[1, 1]}");
            Assert.AreEqual(0.5f, slopemap[1, 1]);
            slopemap = TerrainUtilities.HeightMapToSlopeMap(heightmapY, 1, 1, 1);
            Console.WriteLine($"{slopemap[1, 1]}");
            Assert.AreEqual(0.5f, slopemap[1, 1]);
            slopemap = TerrainUtilities.HeightMapToSlopeMap(heightmapXY, 1, 1, 1);
            Console.WriteLine($"{slopemap[1, 1]}");
            Assert.Greater(Mathf.Sqrt(0.5f), slopemap[1, 1]);
            slopemap = TerrainUtilities.HeightMapToSlopeMap(heightmapY, 2, 1, 1);
            Console.WriteLine($"{slopemap[1, 1]}");
            Assert.AreEqual(Mathf.Atan(2) / Mathf.PI * 2, slopemap[1, 1]);
        }
    }
}
