using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace RogueMaker
{
    public class MapEditor
    {        
        public MapObject PortMap(MapObject map, bool differMaterials)
        {
            map.entities = RemoveNonInvisibleProps(map.entities);
            map.entities = ReplaceRamps(map.entities);
            map.entities = CalculatePropWalls(map, differMaterials);            
            map.blocks = new List<Block> { /*map.blocks[0]*/ };

            return map;
        }

        public List<Entity> CalculatePropWalls(MapObject map, bool differMaterials)
        {
            // CHECKING IF THERE IS NOTHING TO CONVERT
            List<Block> blocks = map.blocks;
            if (blocks == null || blocks.Count == 0) { return map.entities; }

            List<Entity> entities = new List<Entity>();
            int propCount = 1;
            Vector3 position, rotation, scale; 
            float uvScale;
            string name;
            List<Tuple<string, string>> properties;
            Dictionary<Vector3, Block> blockDict = blocks.ToDictionary(b => b.position);
            List<string> materials = MaterialLibrary.Materials;

            // DOING WHILE BLOCKS ARE AVAILABLE TO CONVERT
            while (blockDict.Count > 0)
            {
                Block startingBlock = blockDict.Values
                    .OrderBy(b => b.position.Z)
                    .ThenBy(b => b.position.Y)
                    .ThenBy(b => b.position.X)
                    .First();
                int startingMaterial = differMaterials ? GetMostProbableMaterial(startingBlock, blockDict) : 0;
                position             = new Vector3(startingBlock.position.X * 40, startingBlock.position.Y * 20, startingBlock.position.Z * 40);

                // IF BLOCK DIAGONAL
                if (startingBlock.type == 3)
                {
                    float propHeight    = 1;
                    List<Block> sorted  = new List<Block> { startingBlock };
                    // CHECKING IF NEXT BLOCK IS SAME AS STARTING AND EXIST ON THE MAP (CALCULATING XZ -> XYZ)
                    for (float i = startingBlock.position.Y + 1; blockDict.ContainsKey(new Vector3(startingBlock.position.X, i, startingBlock.position.Z)); i++)
                    {
                        Block nextBlock = blockDict[new Vector3(startingBlock.position.X, i, startingBlock.position.Z)];
                        bool typeDiff   = nextBlock.type == startingBlock.type;                                                
                        bool orient     = nextBlock.orient == startingBlock.orient;
                        if (differMaterials)
                        {
                            if (GetMostProbableMaterial(nextBlock, blockDict) != startingMaterial)
                            {
                                break;
                            }
                        }
                        if (!typeDiff || !orient)
                        {
                            break;
                        }
                        sorted.Add(nextBlock);
                        propHeight++;
                    }
                    // REMOVING CONVERTED BLOCKS
                    foreach (Block block in sorted)
                    {
                        blockDict.Remove(block.position);
                    }

                    // CREATING DIAGONAL WALL
                    name        = $"prop_converted_diagonal_wall_{propCount}";
                    position    = new Vector3(startingBlock.position.X * 40, startingBlock.position.Y * 20, startingBlock.position.Z * 40);
                    propHeight  = (float)(propHeight < 0.25 ? 0.25 : propHeight); 
                    scale       = new Vector3(propHeight / 4, 0.5f, 0.5f);
                    rotation    = new Vector3(0, 0, 90);
                    if      (startingBlock.orient == 0) { rotation.Y = 90f; position.X += 40; }
                    else if (startingBlock.orient == 1) { rotation.Y = 0f; position.Z += 40; position.X += 40; }
                    else if (startingBlock.orient == 2) { rotation.Y = -90f; position.Z += 40; }
                    else if (startingBlock.orient == 3) { rotation.Y = 180; }
                    float uvScaleX = scale.X;
                    if (scale.Y > scale.X)
                    {
                        uvScaleX = scale.Y;
                    }
                    properties = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("model", "blockout_45"),
                        new Tuple<string, string>("no_show", "false"),
                        new Tuple<string, string>("override_effect_0", $"{materials[startingMaterial]}"),
                        new Tuple<string, string>("uv_scale_x", $"{uvScaleX.ToString(CultureInfo.InvariantCulture)}"),
                        new Tuple<string, string>("uv_scale_y", $"{scale.Z.ToString(CultureInfo.InvariantCulture)}"),
                    };                    
                }
                // IF SOLID BLOCK (-> RECTANGLE PROP)
                else
                {
                    // CALCULATING WIDTH (X)
                    List<Block> sortedX = new List<Block> { startingBlock };
                    int propWidth       = 1;
                    for (float i = startingBlock.position.X + 1; blockDict.ContainsKey(new Vector3(i, startingBlock.position.Y, startingBlock.position.Z)); i++)
                    {
                        Block nextBlock = blockDict[new Vector3(i, startingBlock.position.Y, startingBlock.position.Z)];
                        bool typeDiff   = nextBlock.type == startingBlock.type;
                        if (differMaterials)
                        {
                            if (GetMostProbableMaterial(nextBlock, blockDict) != startingMaterial)
                            {
                                break;
                            }
                        }
                        if (!typeDiff)
                        {
                            break;
                        }
                        sortedX.Add(nextBlock);
                        propWidth++;
                    }
                    float scaleX = propWidth;

                    // CALCULATING HEIGHT (X -> XY)
                    List<Block> sortedY = new List<Block>(sortedX);
                    float propHeight    = 1;
                    bool layerCompleteY = true;
                    while (layerCompleteY)
                    {
                        List<Block> currentLayerY = new List<Block>();
                        foreach (Block block in sortedX)
                        {
                            Vector3 nextPos = new Vector3(block.position.X, block.position.Y + propHeight, block.position.Z);
                            if (blockDict.ContainsKey(nextPos))
                            {
                                Block nextBlock = blockDict[nextPos];
                                bool typeDiff   = nextBlock.type == startingBlock.type;
                                if (differMaterials)
                                {
                                    if (GetMostProbableMaterial(nextBlock, blockDict) != startingMaterial)
                                    {
                                        layerCompleteY = false;
                                        break;
                                    }
                                }
                                
                                if (!typeDiff)
                                {
                                    layerCompleteY = false;
                                    break;
                                }
                                currentLayerY.Add(nextBlock);
                            }
                            else
                            {
                                layerCompleteY = false;
                                break;
                            }
                        }
                        if (layerCompleteY)
                        {
                            sortedY.AddRange(currentLayerY);
                            propHeight++;
                        }
                    }
                    float scaleY = propHeight;

                    // CALCULATING DEPTH (XY -> XYZ)
                    List<Block> sortedZ = new List<Block>(sortedY);
                    float propDepth     = 1;
                    bool layerCompleteZ = true;
                    while (layerCompleteZ)
                    {
                        List<Block> currentLayerZ = new List<Block>();
                        foreach (Block block in sortedY)
                        {
                            Vector3 nextPos = new Vector3(block.position.X, block.position.Y, block.position.Z + propDepth);
                            if (blockDict.ContainsKey(nextPos))
                            {
                                Block nextBlock = blockDict[nextPos];
                                bool typeDiff   = nextBlock.type == startingBlock.type;
                                if (differMaterials)
                                {
                                    if (GetMostProbableMaterial(nextBlock, blockDict) != startingMaterial)
                                    {
                                        layerCompleteZ = false;
                                        break;
                                    }
                                }
                                if (!typeDiff)
                                {                                
                                    layerCompleteZ = false;
                                    break;
                                }
                                currentLayerZ.Add(nextBlock);
                            }
                            else
                            {
                                layerCompleteZ = false;
                                break;
                            }
                        }
                        if (layerCompleteZ)
                        {
                            sortedZ.AddRange(currentLayerZ);
                            propDepth++;
                        }
                    }
                    float scaleZ = propDepth;

                    // REMOVING CONVERTED BLOCKS
                    foreach (Block block in sortedZ)
                    {
                        blockDict.Remove(block.position);
                    }

                    // CREATING RECTANGLE PROP
                    name        = $"prop_converted_wall_{propCount}";
                    scale       = new Vector3(scaleX / 2, scaleY / 4, scaleZ / 2); // (default scale of invis_box prop to 1 block)
                    rotation    = new Vector3(0, 0, 0);
                    float uvScaleX = scale.X;
                    if (scale.Y > scale.X)
                    {
                        uvScaleX = scale.Y;
                    }
                    properties  = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>("model", "invisible_box_80_corner"),
                        new Tuple<string, string>("no_show", "false"),
                        new Tuple<string, string>("override_effect_0", $"{materials[startingMaterial]}"),
                        new Tuple<string, string>("uv_scale_x", $"{uvScaleX.ToString(CultureInfo.InvariantCulture)}"),
                        new Tuple<string, string>("uv_scale_y", $"{scale.Z.ToString(CultureInfo.InvariantCulture)}"),
                    };
                }
                entities.Add(new Entity(name, position, rotation, scale, properties));
                propCount++;

                /*// LOG
                Console.WriteLine($"Added new entity - {name}. Position: {position}. Scale: {scale}. Rotation: {rotation}");*/
            }

            foreach (Entity entity in entities)
            {
                map.entities.Add(entity);
            }
            
            return map.entities;
        }

        public List<Entity> RemoveNonInvisibleProps(List<Entity> entities)
        {
            // REMOVING ALL PROPS OTHER THAN INVIS BOX AND RAMPS (?)
            entities.RemoveAll(entity =>
                entity.name.StartsWith("prop_") &&
                !entity.properties.Any(prop => 
                    prop.Item1 == "model" && 
                    prop.Item2.Contains("invis") && 
                    !prop.Item2.Contains("invisible_wall_thin") && 
                    !prop.Item2.Contains("cylinder") &&
                    !prop.Item2.Contains("invisible_box_corner")));
            return entities;
        }

        public List<Entity> ReplaceRamps(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                bool containsInvisModel = entity.properties.Any(prop => 
                    prop.Item1 == "model" &&  
                    prop.Item2.Contains("invisible_ramp") || 
                    prop.Item2.Contains("invisible_box_slope") || 
                    prop.Item2.Contains("invisible_block_diagonal") || 
                    prop.Item2.Contains("invisible_wall_diagonal") ||
                    prop.Item2.Contains("invisible_box_diagonal_corners") ||
                    prop.Item2.Contains("box_corner"));

                if (containsInvisModel)
                {
                    for (int i = 0; i < entity.properties.Count; i++)
                    {
                        if (entity.properties[i].Item1 == "model")
                        {
                            entity.properties[i] = new Tuple<string, string>("model", "blockout_45");
                            entity.properties.Add(new Tuple<string, string>("uv_scale_x", $"{entity.scale.X.ToString(CultureInfo.InvariantCulture)}"));
                            entity.properties.Add(new Tuple<string, string>("uv_scale_y", $"{entity.scale.Z.ToString(CultureInfo.InvariantCulture)}"));
                        }
                    }
                    // TODO position & rotation
                    // entity.rotation = new Vector3(entity.rotation.X, entity.rotation.Y, 0);
                    entity.scale = new Vector3(entity.scale.Y / 2, entity.scale.X / 2, entity.scale.Z / 2);
                }
            }
            return entities;
        }

        
        public bool IsSideVisible(Block block, Vector3 direction, Dictionary<Vector3, Block> blockDict)
        {
            Vector3 neighborPosition = block.position + direction;
            return !blockDict.ContainsKey(neighborPosition);
        }
        public int GetMostProbableMaterial(Block block, Dictionary<Vector3, Block> blockDict)
        {
            // Стороны блока и их направления
            var sides = new Dictionary<string, Vector3>
            {
                { "front", new Vector3(0, 0, 1) },
                { "back", new Vector3(0, 0, -1) },
                { "left", new Vector3(-1, 0, 0) },
                { "right", new Vector3(1, 0, 0) },
                { "top", new Vector3(0, 1, 0) },
                { "bottom", new Vector3(0, -1, 0) }
            };

            // Группировка материалов только видимых сторон
            var visibleMaterials = block.materials
                .Where(kv => IsSideVisible(block, sides[kv.Key], blockDict))
                .GroupBy(kv => kv.Value)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key) // Если более одного, то берем первый
                .FirstOrDefault();

            return visibleMaterials?.Key ?? 0;
        }
    }
}
