using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RogueMaker
{
    public class Block
    {
        public Vector3 position { get; set; }
        public byte type { get; set; }
        public byte[] u1 { get; set; }
        public Dictionary<string, int> materials { get; set; }
        public byte u2 { get; set; }
        public Dictionary<string, Dictionary<string, int>> materialsOffset { get; set; }
        public byte[] u3 { get; set; }
        public byte orient { get; set; }        
        public byte[] u4 { get; set; }

        public Block(
                Vector3 position,
                byte type,
                byte[] u1,
                Dictionary<string, int> materials,
                byte u2,
                Dictionary<string, Dictionary<string, int>> materialsOffset,
                byte[] u3,
                byte orient,
                byte[] u4)
        {
            this.position = position;
            this.type = type;
            this.orient = orient;
            this.materials = materials;
            this.materialsOffset = materialsOffset;
            this.u1 = u1 == null ? new byte[12] : u1;
            this.u2 = u2 == 0 ? new byte() : u2;
            this.u3 = u3 == null ? new byte[6] : u3;
            this.u4 = u4 == null ? new byte[2] : u4;
        }
    }
}
