using UnityEditor;
using UnityEngine;
namespace StephenLujan.TerrainEngine
{
    [CustomEditor(typeof(UnityPerlinNoise))]
    public class UnityNoiseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UnityPerlinNoise target = (UnityPerlinNoise)this.target;
            DrawDefaultInspector();
            EditorGUILayout.Separator();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void DrawPreview(Rect previewArea)
        {
            UnityPerlinNoise target = (UnityPerlinNoise)this.target;
            // previewArea keeps coming as 0,0,1,1?
            Texture2D tex = target.GetTexture((int)previewArea.width, (int)previewArea.height);
            //Texture2D tex = target.GetNoise().GetTexture(100, 100);
            GUI.DrawTexture(previewArea, tex, ScaleMode.StretchToFill, false);
        }
    }
}
