#define FASTNOISE
#if FASTNOISE 
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    public class FastNoiseUnityWrapper : UnityNoiseBase
    {
        [Tooltip("Select a Fast Noise Unity script component from the FastNoise Library")]
        public FastNoiseUnity fastNoiseUnity;

        protected override IInnerNoise GetNoise()
        {
            return new FastNoiseWrapper(fastNoiseUnity.fastNoise);
        }
    }
}
#endif