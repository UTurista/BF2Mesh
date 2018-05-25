using BF2Mesh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2MeshConsole.Templates
{
    public enum MeshType
    {
        BundledMesh         = 0,
        StaticMesh          = 1,
        SkinnedMesh         = 2,

        // some other mesh types?
        DebugSphereMesh     = 3,
        MeshParticleMesh    = 4,
        RoadCompiled        = 5,
    }

    public class BF2GeometryTemplate
    {
        private string _Name;

        public readonly string FileName;

        public string Name
        {
            get { return _Name; }
        }

        public BF2GeometryTemplate(string fileName, MeshType meshType)
        {
            if (meshType == MeshType.DebugSphereMesh) { return; }

            this.FileName = fileName + "." + meshType.ToString().Replace("MeshParticleMesh", "BundledMesh");
            if (!File.Exists(this.FileName))
            {
                //throw new NotImplementedException();
            }
            this._Name = Path.GetFileNameWithoutExtension(this.FileName);
        }

        public bool Update(BF2GeometryTemplate geometryTemplate)
        {
            if (this.FileName != geometryTemplate.FileName) { new NotImplementedException(); }
            return true;
        }
    }
}
