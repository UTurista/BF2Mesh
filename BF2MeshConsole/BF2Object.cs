using BF2MeshConsole.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2MeshConsole
{
    public class BF2Object
    {
        public BF2Geometry Geometry;
        public BF2ObjectTemplate ObjectTemplate;

        public BF2Object(BF2ObjectTemplate objectTemplate)
        {
            this.ObjectTemplate = objectTemplate;
        }

        public BF2Geometry LoadGeometry(BF2GeometryTemplate geometryTemplate)
        {
            this.Geometry = new BF2Geometry(geometryTemplate);

            return this.Geometry;
        }
    }
}
