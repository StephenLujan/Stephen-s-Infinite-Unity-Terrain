using UnityEditor;
using UnityEngine;
namespace StephenLujan.TerrainEngine
{
    [CustomEditor(typeof(UnityNoise))]
    public class UnityNoiseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UnityNoise target = (UnityNoise)this.target;
            DrawDefaultInspector();
            EditorGUILayout.Separator();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void DrawPreview(Rect previewArea)
        {
            UnityNoise target = (UnityNoise)this.target;
            // previewArea keeps coming as 0,0,1,1?
            Texture2D tex = target.GetNoise().GetTexture((int)previewArea.width, (int)previewArea.height);
            //Texture2D tex = target.GetNoise().GetTexture(100, 100);
            GUI.DrawTexture(previewArea, tex, ScaleMode.StretchToFill, false);
        }
    }
}
