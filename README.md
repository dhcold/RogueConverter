# RogueConverter
## What it does
RogueConverter is a C# tool for Diabotical / Diabotical Rogue, that is capable of: 
- Converting all blocks from the map into diagonal/rectrangle props.
- Rescaling converted map (making it bigger or smaller)

## Additional info
1. Converting respects block type (diagonal/solid) and creates corresponded prop
2. You can select if block material should be taken into consideration while converting them (so you can get different materials on converted props aswell. Otherwise every prop will be with same material)
3. It removes any original prop, that starts with `prop_` and isn't `invis` prop
4. All other entities (triggers, tps, lights, etc) stays the same
5. I tried to convert ramps, but they are misplaced for now (maybe i'll found solution for converting any `invis` props into new `blockout` props)

## Download latest .exe
[DOWNLOAD HERE](https://github.com/dhcold/RogueConverter/releases)

## How to use
### Launching app
- Put .exe in a folder with .rbe files (Diabotical map folder - `%appdata%/Diabotical/Maps`). For now it works only with original/parsed Diabotical maps. If you save map in-game (Diabotical Rogue), it will update version and no longer can be loaded in this tool.

![image](https://github.com/dhcold/RogueConverter/assets/30022484/3e1dfb2a-9854-4a7e-a055-ad3b00b5eb9d)
- It will read every .rbe map in that folder, after that you can select which map you want to load
![image](https://github.com/dhcold/RogueConverter/assets/30022484/79864d9d-8f4c-4327-b4f7-807a2de606ef)
### Loading a map
- If map is loaded, you will get short map info. From this point you can select what operation you want to do with the map
![image](https://github.com/dhcold/RogueConverter/assets/30022484/5074248d-239f-4da0-a2e3-f16c29052d08)
### Converting/Rescaling and Saving the results
- After executing operation it will automatically generate new file in Diabotical Rogue folder (`%appdata%/DiaboticalRogue/Maps`). With postfix `_rogue` for converted maps, and `_rescaled` for scaled maps.
![image](https://github.com/dhcold/RogueConverter/assets/30022484/9ab35fd9-11da-445e-a888-34842d09254c)
- You can edit new map right away, by typing command (or copy from app output) to in-game console `/edit newmapname` 

## Example
![image](https://github.com/dhcold/RogueConverter/assets/30022484/1751c6e6-63b7-4659-a86e-b446625474cd) ![image2](https://github.com/dhcold/RogueConverter/assets/30022484/6dbad21a-c521-4095-b8df-54410d251589)

## If you have any questions
Write me in Discord: `@dhcold`

## Special Thanks to
  - `pressOK` - for creating original tool and also helping me a lot.
  - `hst` - for helping me to understand parsing/writing process and helping overall during process. Check out [his tools](https://github.com/marconett/diabotical-tools) too!
  - `GD Studio` - for creating amazing games 🫶

## If you want to do something with the code yourself (examples)
### Loading a Map
```
// Create an instance of MapHandler
MapHandler mapHandler = new MapHandler();

// Load a map from file
MapObject mapObject = mapHandler.LoadMap("path/to/your/mapfile.rbе");

// Access map data
Console.WriteLine($"Map Version: {mapObject.ver}");
Console.WriteLine($"Author: {mapObject.authorName}");
// Access other map components: materials, blocks, entities, etc.
```
### Saving a Map
```

// Save the map to file
mapHandler.SaveMap(mapObject, "path/to/save/mapfile.rbe");
```

