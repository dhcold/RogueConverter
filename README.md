# RogueConverter
RogueConverter is a C# tool that converts all blocks to diagonal/rectrangle props. It reads map data and writing it again.

## Download .exe
[DOWNLOAD HERE](https://github.com/dhcold/RogueConverter/releases/tag/exe)

## How to use
Put .exe in folder with .rbe files (Diabotical map folder - %appdata%/diabotical/maps). It will read every map in that folder, after that you can select which map to port.

## Additional info
1. It respects block type (diagonal/solid) and creates corresponded prop.
2. You can select if block material should be taken into consideration while converting blocks (so you can get different material on converted props as well. If not every prop will be same color)
3. It removes any original prop, that starts with "prop_" and isn't invis prop
4. All other entities (triggers, tps, lights, etc) stays the same
5. I tried to convert ramps, but they are misplaced for now (maybe i'll found solution for converting any 'invis' props into new 'blockout' props)

## Example:
![image](https://github.com/dhcold/RogueConverter/assets/30022484/1751c6e6-63b7-4659-a86e-b446625474cd) ![image2](https://github.com/dhcold/RogueConverter/assets/30022484/6dbad21a-c521-4095-b8df-54410d251589)

## If you have any questions
Write me in Discord: `@dhcold`

## Special Thanks to
  - pressOK - for creating original tool and also helping me a lot.
  - hst - for helping me to understand parsing/writing process and helping overall during process. Check out [his tools](https://github.com/marconett/diabotical-tools) too!
  - GD Studio - for creating amazing games 🫶

## If you want to do something with the code yourself (examples)
### Loading a Map
```
using RogueMaker;

// Create an instance of MapHandler
MapHandler mapHandler = new MapHandler();

// Load a map from file
MapObject mapObject = mapHandler.LoadMap("path/to/your/mapfile.map");

// Access map data
Console.WriteLine($"Map Version: {mapObject.ver}");
Console.WriteLine($"Author: {mapObject.authorName}");
// Access other map components: materials, blocks, entities, etc.
```
### Saving a Map
```
using RogueMaker;

// Create an instance of MapHandler
MapHandler mapHandler = new MapHandler();

// Create a MapObject instance and populate it with data (materials, blocks, entities, etc.)

// Save the map to file
mapHandler.SaveMap(mapObject, "path/to/save/mapfile.map");
```

