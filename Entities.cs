using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RogueMaker
{
    public class Entity
    {
        public string name { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }
        public List<Tuple<string, string>> properties { get; set; }


        public Entity(
            string name,
            Vector3 position,
            Vector3 rotation,
            Vector3 scale,
            List<Tuple<string,string>> properties) 
        {
            this.name = name; 
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.properties = properties;
        }
    }
}
