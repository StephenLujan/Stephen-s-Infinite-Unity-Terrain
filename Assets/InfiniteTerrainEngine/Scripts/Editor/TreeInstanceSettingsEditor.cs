using UnityEditor;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    [CustomPropertyDrawer(typeof(TreeInstanceSettings))]
    public class TreeInstanceSettingsEditor : MapFeatureSettingsEditor
    {
        Editor gameObjectEditor;
        protected override void preview(Rect previewRect)
        {
            GameObject obj = ((TreeInstanceSettings)newSerializedObject.targetObject).Prototype.prefab;

            gameObjectEditor = gameObjectEditor ?? Editor.CreateEditor(obj);
            //gameObjectEditor.OnPreviewGUI(previewRect, new GUIStyle());
            gameObjectEditor.DrawPreview(previewRect);

            //EditorGUI.DrawPreviewTexture(previewRect, texture);
        }
    }
}