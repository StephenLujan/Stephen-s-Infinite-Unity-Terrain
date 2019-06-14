using UnityEditor;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{
    [CustomPropertyDrawer(typeof(MapFeatureSettingsBase), true)]
    public abstract class MapFeatureSettingsEditor : PropertyDrawer
    {
        public const float leftColumnWidth = 80.0f;
        public const float labelHeight = 16.0f;
        public const float sliderHeight = 20.0f;
        public const float verticalMargin = 4.0f;
        public const float horizontalMargin = 6.0f;
        public const float leftHeight = leftColumnWidth + labelHeight + 2 * verticalMargin;
        public const float rightHeight = labelHeight * 4 + sliderHeight * 2 + verticalMargin * 2;

        protected SerializedProperty heightMinProp;
        protected SerializedProperty heightMaxProp;
        protected SerializedProperty slopeMinProp;
        protected SerializedProperty slopeMaxProp;

        // hacks around broken FindPropertyRelative :-(
        protected SerializedObject newSerializedObject;

        private void getProperties(SerializedProperty serializedProperty)
        {
            // Setup the SerializedProperties.
            heightMinProp = serializedProperty.FindPropertyRelative("MinimumHeight");
            heightMaxProp = serializedProperty.FindPropertyRelative("MaximumHeight");
            slopeMinProp = serializedProperty.FindPropertyRelative("MinimumSlope");
            slopeMaxProp = serializedProperty.FindPropertyRelative("MaximumSlope");
            //terrainLayerProp = serializedProperty.FindPropertyRelative("TerrainLayer");
        }

        // hacks https://answers.unity.com/questions/629803/findrelativeproperty-never-worked-for-me-how-does.html
        // hacks https://answers.unity.com/questions/1188088/inspector-cant-find-field-of-scriptableobject.html#answer-1188103
        private void getProperties(SerializedObject serializedObject)
        {
            // Setup the SerializedProperties.
            heightMinProp = serializedObject.FindProperty("MinimumHeight");
            heightMaxProp = serializedObject.FindProperty("MaximumHeight");
            slopeMinProp = serializedObject.FindProperty("MinimumSlope");
            slopeMaxProp = serializedObject.FindProperty("MaximumSlope");
            //terrainLayerProp = serializedObject.FindProperty("TerrainLayer");
        }

        protected abstract void preview(Rect previewRect);

        private void draw(Rect position)
        {
            float x = position.x;
            float y = position.y;
            float width = position.width;


            float rightColumnWidth = position.width - leftColumnWidth - 3 * horizontalMargin;

            EditorGUI.DrawRect(position, new Color(1, 1, 1, 0.2f));

            x += horizontalMargin;
            y += verticalMargin;

            EditorGUI.LabelField(new Rect(x, y, leftColumnWidth, labelHeight), "Feature");

            Rect previewRect = new Rect(x, y + labelHeight, leftColumnWidth, leftColumnWidth);
            //EditorGUI.DrawPreviewTexture(previewRect, ((TerrainLayer)terrainLayerProp.objectReferenceValue).diffuseTexture);
            preview(previewRect);

            x += leftColumnWidth + horizontalMargin;

            // can't put the use .floatValue as ref parameter so we have to make a copy
            float heightMin = heightMinProp.floatValue;
            float heightMax = heightMaxProp.floatValue;
            float slopeMin = slopeMinProp.floatValue;
            float slopeMax = slopeMaxProp.floatValue;

            const string heightTip = "1 represents the highest possible terrain elevation while 0 represents the lowest";

            heightMin = EditorGUI.FloatField(
                new Rect(x, y, rightColumnWidth, labelHeight),
                new GUIContent("Minimum Height", heightTip),
                heightMin);
            y += labelHeight;
            heightMax = EditorGUI.FloatField(
                new Rect(x, y, rightColumnWidth, labelHeight),
                new GUIContent("Maximum Height", heightTip),
                heightMax);
            y += labelHeight;
            EditorGUI.MinMaxSlider(
                new Rect(x, y, rightColumnWidth, sliderHeight),
                ref heightMin,
                ref heightMax,
                0.0f,
                1.0f);
            y += sliderHeight;

            //EditorGUI.LabelField(
            //    new Rect(x, y, rightColumnWidth, labelHeight),
            //    $"Min, Max Slope: {slopeMin}, {slopeMax}");
            //y += labelHeight;

            const string slopeTip = "A slope of 1 means vertical, 0 means flat, and 0.5 means 45°";
            slopeMin = EditorGUI.FloatField(
                new Rect(x, y, rightColumnWidth, labelHeight),
                new GUIContent("Minimum Slope", slopeTip),
                slopeMin);
            y += labelHeight;
            slopeMax = EditorGUI.FloatField(
                new Rect(x, y, rightColumnWidth, labelHeight),
                new GUIContent("Maximum Slope", slopeTip),
                slopeMax);
            y += labelHeight;
            EditorGUI.MinMaxSlider(
                new Rect(x, y, rightColumnWidth, sliderHeight),
                ref slopeMin,
                ref slopeMax,
                0.0f,
                1.0f);
            y += sliderHeight;

            //GUI.Label(new Rect(10, 40, 100, 40), GUI.tooltip);

            heightMinProp.floatValue = heightMin;
            heightMaxProp.floatValue = heightMax;
            slopeMinProp.floatValue = slopeMin;
            slopeMaxProp.floatValue = slopeMax;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            //getProperties(property);
            // hacks around broken FindPropertyRelative :-(
            newSerializedObject = newSerializedObject ?? new SerializedObject(property.objectReferenceValue);
            getProperties(newSerializedObject);

            if (heightMinProp is null ||
                heightMaxProp is null ||
                slopeMinProp is null ||
                slopeMaxProp is null)
            {
                EditorGUI.HelpBox(new Rect(position.x, position.y + 16, position.width, 50),
                    "MapFeatureSettingsEditor could not find properties of TextureSplatSettings.",
                    MessageType.Error);
            }
            else
            {
                draw(position);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            newSerializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Max(leftHeight, rightHeight);
        }
    }
}
