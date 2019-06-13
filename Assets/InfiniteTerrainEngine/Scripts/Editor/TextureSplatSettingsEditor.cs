using UnityEditor;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    [CustomPropertyDrawer(typeof(TextureSplatSettings))]
    public class TextureSplatSettingsEditor : MapFeatureSettingsEditor
    {
        protected override void preview(Rect previewRect)
        {
            SerializedProperty terrainLayerProp = newSerializedObject.FindProperty("Prototype");
            if (terrainLayerProp.objectReferenceValue is null)
            {
                EditorGUI.HelpBox(previewRect, "TextureSplatSettingsEditor encountered a null prototype", MessageType.Error);
                return;
            }
            EditorGUI.DrawPreviewTexture(previewRect, ((TerrainLayer)terrainLayerProp.objectReferenceValue).diffuseTexture);
        }

    }
}
