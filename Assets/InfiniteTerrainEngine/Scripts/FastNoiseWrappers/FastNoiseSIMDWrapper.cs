#define FASTNOISE
#if FASTNOISE 

using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    class FastNoiseSIMDWrapper : IInnerNoise
    {
        private FastNoiseSIMD fastNoiseSIMD;

        public FastNoiseSIMDWrapper(FastNoiseSIMD input)
        {
            fastNoiseSIMD = input;
        }

        public float GetNoise(int x, int y, int z = 0)
        {
            throw new System.NotImplementedException();
        }

        private static void readLinearArray(ref float[,] noiseMap, float[] linear, int xSize, int ySize, bool yxIndexed)
        {
            int index = 0;
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (yxIndexed)
                    {
                        noiseMap[y, x] = linear[index++];
                    }
                    else
                    {
                        noiseMap[x, y] = linear[index++];
                    }
                }
            }
        }

        public void GetNoiseSet(int xStart, int yStart, ref float[,] noiseMap, bool yxIndexed)
        {
            int xSize = noiseMap.GetUpperBound(0) + 1;
            int ySize = noiseMap.GetUpperBound(1) + 1;
            float[] linear = fastNoiseSIMD.GetNoiseSet(xStart, yStart, 0, xSize, ySize, 1);
            readLinearArray(ref noiseMap, linear, xSize, ySize, yxIndexed);
        }

        public void GetSampledNoiseSet(int xStart, int yStart, int sampleScale, ref float[,] noiseMap, bool yxIndexed)
        {
            int xSize = noiseMap.GetUpperBound(0) + 1;
            int ySize = noiseMap.GetUpperBound(1) + 1;
            float[] linear = fastNoiseSIMD.GetSampledNoiseSet(xStart, yStart, 0, xSize, ySize, 1, sampleScale);
            readLinearArray(ref noiseMap, linear, xSize, ySize, yxIndexed);
        }

        public Texture2D GetTexture(int width, int height)
        {
            throw new System.NotImplementedException();
        }

        public void SetSeed(int seed)
        {
            fastNoiseSIMD.SetSeed(seed);
        }
    }

}
#endif
