using UnityEngine;
using Random = System.Random;

namespace StephenLujan.TerrainEngine
{
    public class Noise : INoise
    {
        private float frequency = 0.01f;
        private int octaves = 5;
        private float lacunarity = 2.0f;
        private float gain = 0.5f;
        private Random random;
        private int xOffset;
        private int yOffset;

        /// <summary>
        /// keeps the output within the same range regardless of the number of octaves
        /// </summary>
        private float octavesAdjustment = 1;

        public Noise(int seed, float frequency, int octaves, float lacunarity, float gain)
        {
            this.frequency = frequency;
            this.octaves = octaves;
            this.lacunarity = lacunarity;
            this.gain = gain;
            SetSeed(seed);

            setOctavesAdjustment();
        }

        private void setOctavesAdjustment()
        {
            octavesAdjustment = 1.0f;
            if (octaves > 1)
            {
                for (int octave = 1; octave < octaves; octave++)
                {
                    octavesAdjustment += Mathf.Pow(gain, octave);
                }
            }
        }

        public void SetSeed(int seed)
        {
            random = new Random(seed);
            // Extreme values here cause floating point precision problems in for later calculation.
            // It would be helpful to now at what size the perlin noise pattern in unity repeats.
            // 16 million possibilities will probably suffice
            xOffset = random.Next() % 2000;
            yOffset = random.Next() % 2000;
        }

        public void GetNoiseSet(int xStart, int yStart, ref float[,] noiseMap, bool yxIndexed)
        {
            int xSize = noiseMap.GetUpperBound(0) + 1;
            int ySize = noiseMap.GetUpperBound(1) + 1;
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    if (yxIndexed)
                    {
                        noiseMap[y, x] = GetNoise(x + xStart, y + yStart);
                    }
                    else
                    {
                        noiseMap[x, y] = GetNoise(x + xStart, y + yStart);
                    }
                }
            }
        }

        public float GetNoise(int x, int y, int z = 0)
        {
            x += xOffset;
            y += yOffset;
            float output = 0;
            for (int i = 0; i < octaves; i++)
            {
                // scale and gain should both be a "*1" in the first octave
                float octaveFrequency = frequency * Mathf.Pow(lacunarity, i);
                float octaveGain = Mathf.Pow(gain, i);
                output += Mathf.PerlinNoise(x * octaveFrequency, y * octaveFrequency) * octaveGain;
            }
            return output / octavesAdjustment;
        }

        public Texture2D GetTexture(int width, int height)
        {
            Texture2D output = new Texture2D(width, height);
            Color[] pixels = new Color[output.width * output.height];

            int index = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = (GetNoise(x, y) + 1.0f) / 2.0f;

                    //pixels[index++] = new Color(1.0f, 0, 0);
                    pixels[index++] = new Color(noise, noise, noise);
                }
            }
            output.SetPixels(pixels);
            output.Apply();
            return output;
        }

        public void GetSampledNoiseSet(int xStart, int yStart, int sampleScale, ref float[,] noiseMapbool, bool yxIndexed = false)
        {
            //TODO: actual sampling... if it will even improve performance
            throw new System.NotImplementedException();
        }
    }
}
