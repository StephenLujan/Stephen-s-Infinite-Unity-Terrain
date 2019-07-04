#define FASTNOISE
#if FASTNOISE 

using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    public class FastNoiseWrapper : IInnerNoise
    {
        private FastNoise fastNoise;

        public FastNoiseWrapper(FastNoise fastNoise)
        {
            this.fastNoise = fastNoise;
        }

        public float GetNoise(int x, int y, int z = 0)
        {
            return fastNoise.GetNoise(x, y, z);
        }

        public void GetNoiseSet(int xStart, int yStart, ref float[,] noiseMap, bool yxIndexed)
        {
            int xSize = noiseMap.GetUpperBound(0) + 1;
            int ySize = noiseMap.GetUpperBound(1) + 1;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float value = fastNoise.GetNoise(x + xStart, y + yStart);
                    if (yxIndexed)
                    {
                        noiseMap[y, x] = value;
                    }
                    else
                    {
                        noiseMap[x, y] = value;
                    }
                }
            }
        }

        public void GetSampledNoiseSet(int xStart, int yStart, int sampleScale, ref float[,] noiseMap, bool yxIndexed)
        {
            throw new System.NotImplementedException();
        }

        public Texture2D GetTexture(int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public void SetSeed(int seed)
        {
            fastNoise.SetSeed(seed);
        }
    }
}
#endif
