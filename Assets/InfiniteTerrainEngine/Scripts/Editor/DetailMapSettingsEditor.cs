using UnityEditor;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    [CustomPropertyDrawer(typeof(DetailMapSettings))]
    public class DetailMapSettingsEditor : MapFeatureSettingsEditor
    {
        protected override void preview(Rect previewRect)
        {
            Texture2D texture = ((DetailMapSettings)newSerializedObject.targetObject).Prototype?.prototypeTexture;
            if (texture is null)
            {
                EditorGUI.HelpBox(previewRect, "DetailProtype should not be null here", MessageType.Error);
            }
            else
            {
                EditorGUI.DrawPreviewTexture(previewRect, texture);
            }
        }
    }
}
