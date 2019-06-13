using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StephenLujan.TerrainEngine
{

    [CustomEditor(typeof(TerrainGenerator))]
    public class TerrainGeneratorEditor : Editor
    {
        private static SettingsType[] updateSettingsOnTarget<SettingsType, SourceType>(
            SettingsType[] settings,
            IEnumerable<SourceType> source,
            string settingsName,
            string sourceName,
            Func<SettingsType, SourceType, bool> comparator,
            Func<SourceType, SettingsType> creator,
            Func<SettingsType[], IEnumerable<SourceType>, bool> sequenceComparator)
            where SettingsType : MapFeatureSettings<SourceType>
        {
            if (source is null)
            {
                Debug.LogError($"Could not get {sourceName} in TerrainGeneratorEditor");
                return settings;
            }

            if (settings is null || !sequenceComparator(settings, source))
            {
                Debug.Log($"TerrainGeneratorEditor found sequence of {settings?.Count()} {settingsName} did not match the {source.Count()} {sourceName}.");

                // Build new splat settings array based on template's TerrainLayers
                settings = source.Select(sourceObject =>
                {
                    // reuse existing settings if they exist for current TerrainLayer
                    SettingsType output = settings?.FirstOrDefault(setting => comparator(setting, sourceObject));

                    // otherwise create a new one from current TerrainLayer
                    if (output is null)
                    {
                        output = creator(sourceObject);
                    }
                    else
                    {
                        // even if were were equal, make sure we refer to the current object
                        // ...these darn things seem rather transient in unity
                        output.Prototype = sourceObject;
                    }
                    return output;
                }).ToArray();
            }
            return settings;
        }

        public void updateTextureSplatSettingsOnTarget(TerrainGenerator myTarget)
        {
            TextureSplatSettings[] splatSettings = myTarget.TextureSplatSettings;
            myTarget.TextureSplatSettings = updateSettingsOnTarget(
                settings: splatSettings,
                source: myTarget?.TemplateTerrain?.terrainData?.terrainLayers,
                settingsName: "Texture Splat Settings",
                sourceName: "Template TerrainLayers",
                comparator: (settings, terrainLayer) => settings?.Prototype == terrainLayer,
                creator: templateTerrainLayer =>
                {
                    TextureSplatSettings output = (TextureSplatSettings)CreateInstance(typeof(TextureSplatSettings));
                    output.Prototype = templateTerrainLayer;
                    output.name = $"{templateTerrainLayer.name} Splat Settings";
                    return output;
                },
                sequenceComparator: (settings, terrainLayers) => settings.Select(x => x?.Prototype).SequenceEqual(terrainLayers)
                );
        }

        public void updateDetailMapSettings(TerrainGenerator myTarget)
        {
            myTarget.DetailMapSettings = updateSettingsOnTarget(
                settings: myTarget.DetailMapSettings,
                source: myTarget?.TemplateTerrain?.terrainData?.detailPrototypes,
                settingsName: "Terrain Detail Map Settings",
                sourceName: "Template DetailPrototypes",
                comparator: (settings, prototype) => prototype.Equals(settings?.Prototype),
                creator: source =>
                {
                    DetailMapSettings output = (DetailMapSettings)CreateInstance(typeof(DetailMapSettings));
                    output.Prototype = source;
                    //output.name = $"DetailMapSettings {source.ToString()}";
                    return output;
                },
                sequenceComparator: (settings, sources) => settings.Select(x => x?.Prototype).SequenceEqual(sources)
                //|| settings.Select(x => x?.DetailPrototype?.prototype).SequenceEqual(sources.Select(x => x.prototype))
                );
        }

        public void updateTreeInstanceSettings(TerrainGenerator myTarget)
        {
            myTarget.TreeInstanceSettings = updateSettingsOnTarget(
                settings: myTarget.TreeInstanceSettings,
                source: myTarget?.TemplateTerrain?.terrainData?.treePrototypes,
                settingsName: "Terrain Tree Instance Settings",
                sourceName: "Template TreePrototypes",
                comparator: (settings, prototype) => prototype.Equals(settings?.Prototype),
                creator: source =>
                {
                    TreeInstanceSettings output = (TreeInstanceSettings)CreateInstance(typeof(TreeInstanceSettings));
                    output.Prototype = source;
                    //output.name = $"DetailMapSettings {source.ToString()}";
                    return output;
                },
                sequenceComparator: (settings, sources) => settings.Select(x => x?.Prototype).SequenceEqual(sources)
                );
        }

        public override void OnInspectorGUI()
        {
            TerrainGenerator terrainGenerator = (TerrainGenerator)target;

            // update target
            updateTextureSplatSettingsOnTarget(terrainGenerator);
            updateDetailMapSettings(terrainGenerator);
            updateTreeInstanceSettings(terrainGenerator);
            // pull updated texture splat settings from target into SerializedObject
            serializedObject.Update();
            GameObject prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(target);
            if (prefab != null)
            {
                EditorGUILayout.HelpBox(
                    $"Terrain texture splat settings do not persist on prefabs." +
                    $" You must unpack the prefab \"{prefab.name}\" for texure splat settings to work.",
                    MessageType.Warning);
            }
            // now draw the inspector
            DrawDefaultInspector();
            // post changes to SerializedObject back
            serializedObject.ApplyModifiedProperties();
        }
    }


    //[CustomEditor(typeof(TerrainGenerator))]
    //[CanEditMultipleObjects]
    //public class TerrainGeneratorEditor : Editor
    //{

    //    private SerializedProperty terrainNoise;
    //    private SerializedProperty templateTerrain;
    //    private SerializedProperty textureSplatSettingsProp;
    //    private TextureSplatSettings[] splatSettings;

    //    void OnEnable()
    //    {
    //        // Setup the SerializedProperties.
    //        terrainNoise = serializedObject.FindProperty("TerrainNoise");
    //        templateTerrain = serializedObject.FindProperty("TemplateTerrain");
    //        textureSplatSettingsProp = serializedObject.FindProperty("textureSplatSettings");
    //    }

    //    public override void OnInspectorGUI()
    //    {
    //        //do this first to make sure you have the latest version
    //        serializedObject.Update();

    //        //DrawDefaultInspector();

    //        // copy TextureSplatSettings from SerializedProperty
    //        int arraySize = textureSplatSettingsProp.arraySize;
    //        splatSettings = new TextureSplatSettings[arraySize];
    //        for (int i = 0; i < arraySize; i++)
    //        {
    //            splatSettings[i] = (TextureSplatSettings)(textureSplatSettingsProp.GetArrayElementAtIndex(i).objectReferenceValue);
    //        }
    //        // filter out nulls
    //        //splatSettings = splatSettings.Except(null).ToArray();

    //        //var textures = myTarget.TemplateTerrain.terrainData.splatPrototypes.Select(x=> x.texture);
    //        IEnumerable<Texture2D> textures = ((Terrain)(templateTerrain.objectReferenceValue))?.terrainData?.terrainLayers?.Select(x => x.diffuseTexture);
    //        if (textures is null)
    //        {
    //            return;
    //        }

    //        //Debug.Log($"TerrainGeneratorEditor found {splatSettings.Count()} splat settings for {textures.Count()} textures.");
    //        //Debug.Log($"myTarget.TemplateTerrain.terrainData.alphamapLayers: {myTarget.TemplateTerrain.terrainData.alphamapLayers}");

    //        // Update for textures on terrain template
    //        if (!splatSettings.Select(x => x?.Preview).SequenceEqual(textures))
    //        {
    //            Debug.Log($"TerrainGeneratorEditor found sequence of {splatSettings.Count()} splat settings did not match the {textures.Count()} terrain textures.");
    //            splatSettings = textures.Select(tex =>
    //            {
    //                // reuse existing settings if possible, else make a new one
    //                return splatSettings.SingleOrDefault(settings => settings?.Preview == tex)
    //                    ?? new TextureSplatSettings() { Preview = tex };
    //            }).ToArray();

    //            // update serialized property array
    //            textureSplatSettingsProp.arraySize = 0;//splatSettings.Length;
    //            for (int i = 0; i < splatSettings.Length; i++)
    //            {
    //                textureSplatSettingsProp.GetArrayElementAtIndex(i).objectReferenceValue = splatSettings[i];
    //            }
    //        }
    //        serializedObject.ApplyModifiedProperties();

    //        DrawDefaultInspector();

    //        serializedObject.ApplyModifiedProperties();
    //    }
    //}

}


