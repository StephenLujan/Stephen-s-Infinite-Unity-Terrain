using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    /// <summary>
    /// 
    /// </summary>
    public interface INoise
    {
        void SetSeed(int seed);

        /// <summary>
        /// fills noiseMap array with noise 
        /// </summary>
        void GetNoiseSet(int xStart, int yStart, ref float[,] noiseMap, bool yxIndexed = false);

        float GetNoise(int x, int y, int z = 0);

        void GetSampledNoiseSet(int xStart, int yStart, int sampleScale, ref float[,] noiseMapbool, bool yxIndexed = false);

        Texture2D GetTexture(int width, int height);
    }

    public abstract class UnityNoiseBase : MonoBehaviour
    {
        public abstract INoise GetNoise();
    }
}
