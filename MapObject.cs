using RogueMaker;
using System;
using System.Collections.Generic;

public class MapObject
{
    // HEADER
    public string rebm { get; set; }
    public int ver { get; set; }
    public byte[] u1 { get; set; }

    // AUTHOR
    public string authorName { get; set; }
    public byte[] u2 { get; set; }

    // GZIP
    public List<Material> materials { get; set; }
    public byte[] u3 { get; set; }
    public List<Block> blocks { get; set; }    
    public List<byte[]> u4 { get; set; }
    public List<Entity> entities { get; set; }    
    public byte[] u5 { get; set; }

    public MapObject()
    {
        materials = new List<Material>();
        blocks = new List<Block>();
        entities = new List<Entity>();
        u4 = new List<byte[]>();
    }
}