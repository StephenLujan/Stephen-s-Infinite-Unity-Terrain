#define FASTNOISE
#if FASTNOISE
namespace StephenLujan.TerrainEngine
{
    public class FastNoiseSIMDUnityWrapper : UnityNoiseBase
    {
        public FastNoiseSIMDUnity fastNoiseSIMDUnity;

        private FastNoiseSIMDWrapper fastNoiseSIMDWrapper;

        //public void Awake()
        //{
        //    fastNoiseSIMDWrapper = new FastNoiseSIMDWrapper(fastNoiseSIMDUnity.fastNoiseSIMD);
        //}

        public void Start()
        {
            fastNoiseSIMDWrapper = new FastNoiseSIMDWrapper(fastNoiseSIMDUnity.fastNoiseSIMD);
        }

        public override INoise GetNoise()
        {
            return fastNoiseSIMDWrapper;
        }
    }
}
#endif