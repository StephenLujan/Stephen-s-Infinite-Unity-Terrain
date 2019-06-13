#define FASTNOISE
#if FASTNOISE 
namespace StephenLujan.TerrainEngine
{
    public class FastNoiseUnityWrapper : UnityNoiseBase
    {
        public FastNoiseUnity fastNoiseUnity;

        public override INoise GetNoise()
        {
            return new FastNoiseWrapper(fastNoiseUnity.fastNoise);
        }
    }
}
#endif