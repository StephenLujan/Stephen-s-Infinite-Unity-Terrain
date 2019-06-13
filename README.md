# Stephen's Infinite Unity Terrain

Terrain generator and manager creating infinite terrain in the Unity game engine. This asset will utilitize Unity's built-in Terrain objects to support all features provided by Unity for terrain. The TerrainManager will request Unity Terrain "tiles" from the TerrainGenerator within a radius of the player, loading new tiles and unloading old tiles as the player moves. 

### Features
* Using multithreading and parallelism as much as possible.
* The terrain generator can utilize noise from either the Unity engine or from [Jordan Peck's FastNoise] (https://assetstore.unity.com/packages/tools/particles-effects/fastnoise-70706). 
* Unity Terrain are created procedurally, with heightmaps, texture splat maps, and detail maps, by the TerrainGenerator.
* The TerrainManager, TerrainGenerator, and various Noise implentations all have many customizable settings controllable through custom Unity editors.

### Notes For Use 
* Currently a single unactive (not visible) unity terrain is used as a template for terrain tiles to control aspects such as terrain tile width, length, height, terrain layers, detail prototypes, tree prototypes, etc. 

### Technical Notes
(For those who want to work on the scripts themselves)
* Most Unity engine code is not yet threadsafe. To get around this Unity coroutines, which run in the main thread, are used to handle necessary interactions with the Unity engine. These in turn run many c# tasks, which the coroutines yield on until the tasks are completed, allowing the results to be picked up in later frames. Queues are used to send requests for terrain tiles from the TerrainManager to the TerrainGenerator, and to feed results from the TerrainGenerator back to the TerrainManager.
