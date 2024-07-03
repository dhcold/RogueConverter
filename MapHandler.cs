using System.IO.Compression;
using System.Numerics;
using System.Text;

namespace RogueMaker
{
    public class MapHandler
    {
        public MapObject LoadMap(string filePath)
        {
            MapObject mapObject = new MapObject();

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // READING HEADER
                mapObject.rebm = DecodeString(br.ReadBytes(4));
                mapObject.ver = DecodeInt(br.ReadBytes(4));
                mapObject.u1 = br.ReadBytes(8);

                // AUTHOR                
                int authorLength = DecodeInt(br.ReadBytes(4));
                mapObject.authorName = DecodeString(br.ReadBytes(authorLength));
                mapObject.u2 = br.ReadBytes(8);

                // READING BODY OF THE MAP             
                using (var decompressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(fs, CompressionMode.Decompress, true))
                    {
                        gzipStream.CopyTo(decompressedStream);
                    }
                    decompressedStream.Position = 0;
                    LoadDecompressedData(mapObject, decompressedStream);
                }                
            }

            return mapObject;
        }

        private void LoadDecompressedData(MapObject mapObject, Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                // MATERIALS
                int materialCount = br.ReadByte() - 1;
                
                for (int i = 0; i < materialCount; i++)
                {
                    var nameLength = DecodeInt(br.ReadBytes(4));
                    var name = DecodeString(br.ReadBytes(nameLength));
                    Material material = new Material(name);
                    mapObject.materials.Add(material);
                }  

                mapObject.u3 = br.ReadBytes(4);

                // BLOCKS                
                int blockCount = DecodeInt(br.ReadBytes(4));
                for (int i = 0; i < blockCount; i++)
                {
                    Block block = new Block(
                        // position
                        new Vector3 (DecodeInt(br.ReadBytes(4)), DecodeInt(br.ReadBytes(4)), DecodeInt(br.ReadBytes(4))), 
                        // type
                        br.ReadByte(),
                        // u1
                        br.ReadBytes(12),
                        // materials
                        new Dictionary<string, int>
                        {
                            {"front", br.ReadByte()}, 
                            {"left", br.ReadByte()},
                            {"back", br.ReadByte()},
                            {"right", br.ReadByte()},
                            {"top", br.ReadByte()},
                            {"bottom", br.ReadByte()}
                        },
                        // u2
                        br.ReadByte(),
                        // materials offset
                        new Dictionary<string, Dictionary<string, int>>
                        {
                            {"front", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }}, 
                            {"left", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }},
                            {"back", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }},
                            {"right", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }},
                            {"top", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }},
                            {"bottom", new Dictionary<string, int> {{"x", br.ReadByte() }, {"y", br.ReadByte() } }},
                        },
                        //u3
                        br.ReadBytes(6),
                        //orient
                        br.ReadByte(),
                        //u4
                        br.ReadBytes(2)
                    );
                    mapObject.blocks.Add(block);
                }

                // UNKNOWN 4
                int u4Count = DecodeInt(br.ReadBytes(4));
                mapObject.u4 = new List<byte[]>();
                for (int i = 0; i < u4Count; i++)
                {
                    mapObject.u4.Add(br.ReadBytes(16));
                }

                // ENTITIES
                int entityCount = DecodeInt(br.ReadBytes(4));
                mapObject.entities = new List<Entity>();
                for (int i = 0; i < entityCount; i++)
                {
                    int nameLength = DecodeInt(br.ReadBytes(4));
                    string name = DecodeString(br.ReadBytes(nameLength));
                    Vector3 position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    Vector3 rotation = new Vector3(RadToDeg(br.ReadSingle()), RadToDeg(br.ReadSingle()), RadToDeg(br.ReadSingle()));
                    Vector3 scale = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    int propertiesCcount = DecodeInt(br.ReadBytes(4));
                    List<Tuple<string, string>> properties = new List<Tuple<string, string>>();
                    for (int j = 0; j < propertiesCcount; j++)
                    {
                        int propNameLen = DecodeInt(br.ReadBytes(4));
                        string propertyName = DecodeString(br.ReadBytes(propNameLen));

                        int valLen = DecodeInt(br.ReadBytes(4));
                        string propertyValue = DecodeString(br.ReadBytes(valLen));
                        Tuple<string, string> property = new Tuple<string, string>(propertyName, propertyValue);

                        properties.Add(property);
                    }
                    Entity entity = new Entity(name, position, rotation, scale, properties);
                    mapObject.entities.Add(entity);
                }
                
                // REST OF THE MAP
                mapObject.u5 = br.ReadBytes((int)br.BaseStream.Length);
            }
        }

        public void SaveMap(MapObject mapObject, string targetFilePath)
        {
            // Create the directory if it doesn't exist
            string targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            using (var fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                // Write the uncompressed part first
                bw.Write(EncodeStr(mapObject.rebm));
                bw.Write(EncodeNum(mapObject.ver));
                bw.Write(mapObject.u1.ToArray());

                bw.Write(EncodeNum(mapObject.authorName.Count()));
                bw.Write(EncodeStr(mapObject.authorName));
                bw.Write(mapObject.u2.ToArray());

                // Prepare the memory stream for the compressed part
                using (var ms = new MemoryStream())
                {
                    using (var compressedStream = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        using (var compressedWriter = new BinaryWriter(compressedStream))
                        {
                            // MATERIALS
                            var materialCount = mapObject.materials.Count;
                            compressedWriter.Write((byte)(materialCount + 1));
                            foreach (Material material in mapObject.materials)
                            {
                                int nameLength = material.name.Length;
                                compressedWriter.Write(EncodeNum(nameLength));
                                compressedWriter.Write(EncodeStr(material.name));
                            }

                            // UNKNOWN 3
                            compressedWriter.Write(mapObject.u3.ToArray());

                            // BLOCKS
                            compressedWriter.Write(EncodeNum(mapObject.blocks.Count()));
                            foreach (Block block in mapObject.blocks)
                            {
                                compressedWriter.Write(EncodeNum((int)block.position.X));
                                compressedWriter.Write(EncodeNum((int)block.position.Y));
                                compressedWriter.Write(EncodeNum((int)block.position.Z));
                                compressedWriter.Write(block.type);
                                compressedWriter.Write(block.u1);
                                Dictionary<string, int> mats = block.materials;
                                compressedWriter.Write((byte)mats["front"]);
                                compressedWriter.Write((byte)mats["left"]);
                                compressedWriter.Write((byte)mats["back"]);
                                compressedWriter.Write((byte)mats["right"]);
                                compressedWriter.Write((byte)mats["top"]);
                                compressedWriter.Write((byte)mats["bottom"]);
                                compressedWriter.Write(block.u2);
                                Dictionary<string, Dictionary<string, int>> matOffs = block.materialsOffset;
                                compressedWriter.Write((byte)matOffs["front"]["x"]);
                                compressedWriter.Write((byte)matOffs["front"]["y"]);
                                compressedWriter.Write((byte)matOffs["left"]["x"]);
                                compressedWriter.Write((byte)matOffs["left"]["y"]);
                                compressedWriter.Write((byte)matOffs["back"]["x"]);
                                compressedWriter.Write((byte)matOffs["back"]["y"]);
                                compressedWriter.Write((byte)matOffs["right"]["x"]);
                                compressedWriter.Write((byte)matOffs["right"]["y"]);
                                compressedWriter.Write((byte)matOffs["top"]["x"]);
                                compressedWriter.Write((byte)matOffs["top"]["y"]);
                                compressedWriter.Write((byte)matOffs["bottom"]["x"]);
                                compressedWriter.Write((byte)matOffs["bottom"]["y"]);                                
                                compressedWriter.Write(block.u3);
                                compressedWriter.Write(block.orient);
                                compressedWriter.Write(block.u4);
                            }

                            // UNKNOWN 4
                            compressedWriter.Write(EncodeNum(mapObject.u4.Count));
                            foreach (var u4 in mapObject.u4)
                            {
                                compressedWriter.Write(u4);
                            }

                            // ENTITIES
                            compressedWriter.Write(EncodeNum(mapObject.entities.Count()));
                            foreach (var entity in mapObject.entities)
                            {
                                compressedWriter.Write(EncodeNum(entity.name.Count()));
                                compressedWriter.Write(EncodeStr(entity.name));
                                compressedWriter.Write(EncodeNum(entity.position.X));
                                compressedWriter.Write(EncodeNum(entity.position.Y));
                                compressedWriter.Write(EncodeNum(entity.position.Z));
                                compressedWriter.Write(DegToRad(entity.rotation.X));
                                compressedWriter.Write(DegToRad(entity.rotation.Y));
                                compressedWriter.Write(DegToRad(entity.rotation.Z));
                                compressedWriter.Write(EncodeNum(entity.scale.X));
                                compressedWriter.Write(EncodeNum(entity.scale.Y));
                                compressedWriter.Write(EncodeNum(entity.scale.Z));
                                compressedWriter.Write(EncodeNum(entity.properties.Count));
                                List<Tuple<string, string>> properties = entity.properties;
                                foreach (Tuple<string, string> property in properties)
                                {
                                    compressedWriter.Write(EncodeNum(property.Item1.Count()));
                                    compressedWriter.Write(EncodeStr(property.Item1));
                                    compressedWriter.Write(EncodeNum(property.Item2.Count()));
                                    compressedWriter.Write(EncodeStr(property.Item2));
                                }
                            }

                            // REST OF THE MAP (UNKNOWN 5)
                            compressedWriter.Write(mapObject.u5.ToArray());
                        }
                    }

                    // Write the compressed data to the file
                    bw.Write(ms.ToArray());
                }
            }
        }

        public int DecodeInt(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }
        private string DecodeString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private float DegToRad(float deg)
        {
            return (float)(Math.PI / 180.0 * deg);
        }

        private float RadToDeg(float rad)
        {
            return (float)(rad * 180.0 / Math.PI);
        }

        private byte[] EncodeNum(int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            //if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private byte[] EncodeNum(float i)
        {
            byte[] b = BitConverter.GetBytes(i);
            //if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private byte[] EncodeStr(string s)
        {
            byte[] b = Encoding.ASCII.GetBytes(s);
            //if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }
    }
}
