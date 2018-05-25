using BF2Mesh;
using BF2MeshConsole.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2MeshConsole
{
    public class BF2Geometry
    {
        public readonly string Name;
        private bf2mesh mesh;

        public BF2Geometry(BF2GeometryTemplate geometryTemplate)
        {
            this.Name = geometryTemplate.Name;
            this.Load(geometryTemplate.FileName);
        }

        public void Load(string fileName)
        {
            this.mesh = new bf2mesh();
            this.mesh.Load(fileName);
        }

        public List<string> GetTextures()
        {
            return mesh.GetTextureNames();
        }
    }
}
