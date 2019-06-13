using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    /// <summary>
    /// Limits the propogation of map features based on slope and height
    /// </summary>
    [System.Serializable]
    public class MapFeatureSettingsBase : ScriptableObject
    {
        //public string Name = string.Empty;
        [SerializeField]
        public float MinimumHeight;
        [SerializeField]
        public float MaximumHeight = 1.0f;
        [SerializeField]
        public float MinimumSlope;
        [SerializeField]
        public float MaximumSlope = 1.0f;
    }

    public class MapFeatureSettings<T> : MapFeatureSettingsBase
    {
        public T Prototype;
    }
}
