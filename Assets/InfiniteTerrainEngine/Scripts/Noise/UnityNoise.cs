namespace StephenLujan.TerrainEngine
{
    public class UnityNoise : UnityNoiseBase
    {
        public int seed = 0;
        public float frequency = 0.01f;
        public int octaves = 3;
        public float lacunarity = 2.0f;
        public float gain = 0.5f;
        private Noise noise;

        public override INoise GetNoise()
        {
            initializeNoise();
            return noise;
        }

        public void Awake()
        {
            initializeNoise();
        }

        public void Start()
        {
            initializeNoise();
        }

        private void initializeNoise()
        {
            if (noise is null)
            {
                noise = new Noise(seed, frequency, octaves, lacunarity, gain);
            }
        }
    }
}
