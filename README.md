# RogueConverter
## What it does
RogueConverter is a C# tool for Diabotical / Diabotical Rogue, that converts all blocks from the map into diagonal/rectrangle props. It reads map data and writing it again.

## Download .exe
[DOWNLOAD HERE](https://github.com/dhcold/RogueConverter/releases/tag/exe)

## How to use
- Put .exe in a folder with .rbe files (Diabotical map folder - `%appdata%/Diabotical/Maps`)
  
  ![image](https://github.com/dhcold/RogueConverter/assets/30022484/f44553c9-f36d-4454-aaa1-8de6a8fc15c4)
- It will read every .rbe map in that folder, after that you can select which map to convert
  ![image](https://github.com/dhcold/RogueConverter/assets/30022484/6a315065-a71c-4f16-9d4a-b49bf73242c7)
- After executing it will automatically generate new file in Diabotical Rogue folder (`%appdata%/DiaboticalRogue/Maps`) with postfix `_rogue`
  ![image](https://github.com/dhcold/RogueConverter/assets/30022484/8406f085-0273-4814-8e6c-4dc9a2f70ca9)
- You can edit ported map right away, by typing command (or copy from app output) to in-game console `/edit mapname_rogue` 


## Additional info
1. It respects block type (diagonal/solid) and creates corresponded prop
2. You can select if block material should be taken into consideration while converting blocks (so you can get different materials on converted props aswell. Otherwise every prop will be with same material)
3. It removes any original prop, that starts with `prop_` and isn't `invis` prop
4. All other entities (triggers, tps, lights, etc) stays the same
5. I tried to convert ramps, but they are misplaced for now (maybe i'll found solution for converting any `invis` props into new `blockout` props)

## Example:
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

