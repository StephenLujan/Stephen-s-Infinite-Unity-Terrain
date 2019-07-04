#define FASTNOISE
#if FASTNOISE
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    public class FastNoiseSIMDUnityWrapper : UnityNoiseBase
    {
        [Tooltip("Select a Fast Noise SIMD Unity script component from the FastNoise Library")]
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

        protected override IInnerNoise GetNoise()
        {
            return fastNoiseSIMDWrapper;
        }
    }
}
#endif