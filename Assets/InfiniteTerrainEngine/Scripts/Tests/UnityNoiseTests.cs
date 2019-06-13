using NUnit.Framework;
using StephenLujan.TerrainEngine;
using System.Collections;
using UnityEngine.TestTools;

namespace Assets.InfiniteTerrainEngine.Scripts.Tests
{
    public class UnityNoiseTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void UnityNoiseTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator UnityNoiseTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [UnityTest]
        public IEnumerator NoiseSetNotFlat()
        {
            Noise noise = new Noise(0, 0.1f, 3, 2.0f, 0.5f);
            float[,] output = new float[3, 3];
            noise.GetNoiseSet(0, 0, ref output, true);
            Assert.AreNotEqual(output[0, 0], output[2, 0]);
            Assert.AreNotEqual(output[0, 0], output[0, 2]);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetNoiseNotFlat()
        {
            Noise noise = new Noise(0, 0.1f, 3, 2.0f, 0.5f);

            Assert.AreNotEqual(noise.GetNoise(0, 0), noise.GetNoise(9999, 9999));
            Assert.AreNotEqual(noise.GetNoise(0, 0), noise.GetNoise(0, 1));
            Assert.AreNotEqual(noise.GetNoise(0, 0), noise.GetNoise(1, 0));
            yield return null;
        }
    }
}
