# Stephen's Infinite Unity Terrain

Terrain generator and manager creating infinite terrain in the Unity game engine. This asset will utilitize Unity's built-in Terrain objects to support all features provided by Unity for terrain. Terrain will be continuously loaded and unloaded around the player using multithreaded parallelism as much as possible. This will be a unity store asset, but is provided free here for reading and private non-commercial use.

### Features
* Effectively infinite contiguous terrain (limited by floating point extremes)
* Use a custom seed with the same settings get the same terrain in the same places, or random seeds.
* Using multithreading and parallelism as much as possible.
* Uses built in Unity terrain, not meshes, allowing for all existing features, familiarity with those who have used it, and better compatibility with existing custom terrain shaders and other resources intended for Unity Terrains.
* The TerrainManager, TerrainGenerator, and various Noise implentations all have many customizable settings controllable through custom Unity editors.
* Generates realistic heightmaps from noise
* Generates realistic texture splat maps based on terrain height and slope
* Generates Detail maps (grass etc.) based on terrain height and slope **in progress**
* Generates SpeedTrees  based on terrain height and slope **in progress**
* The terrain generator can utilize noise from either the Unity engine or from [Jordan Peck's FastNoise] (https://assetstore.unity.com/packages/tools/particles-effects/fastnoise-70706). 
* Terrain tile caching. (A configurable number of the terrain tiles removed most recently from render for being to far are kept in memory. This prevents the same tiles from being generated over and over if a player is moving around a lot in a small area.)

### Planned Features
(in rough order of current priorities)
* Easy hooks for loading and unloading game specific content and behavior
* Saving terrain to hard-drive allowing for in-game terrain deformations and changes to be persistent, as well as allowing for custom maps in the middle of infinite terrain
* better functions to add features to terrain programmatically e.g. add structures but flatten the terrain underneath them.
* Biome based terrain generation
* custom terrain shaders
   * Get rid of texture stretching on steep terrain
   * Perform texture splat mapping on the gpu on the fly to reduce cpu work in creating new tiles
   * Better texture blending, PBR textures
   * Allow Macro textures and detail textures
* Improve heightmap realism beyond what's possible with just fractal noise, mimicking erosion etc.
* Procedural creation of rivers and streams
* Take user requests.

### Notes For Use 
* Currently a single unactive (not visible) unity terrain is used as a template for terrain tiles to control aspects such as terrain tile width, length, height, terrain layers, detail prototypes, tree prototypes, etc. 

### Technical Notes
(For those who want to work on the scripts themselves)
* Most Unity engine code is not yet threadsafe. To get around this Unity coroutines, which run in the main thread, are used to handle necessary interactions with the Unity engine. These in turn run many .net tasks doing as much of the work as possible, which the coroutines yield on until the tasks are completed, allowing the results to be picked up in later frames. (I'm not sure how the rest of you handle parallelism in Unity, but this is what I came up with.)
* Queues are used to send requests for terrain tiles from the TerrainManager to the TerrainGenerator, and to feed results from the TerrainGenerator back to the TerrainManager.
