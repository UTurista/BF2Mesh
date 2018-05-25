using BF2MeshConsole.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BF2MeshConsole
{
    public class BF2Level
    {
        // refactor to something like level.mod?
        // done in loading with source choosing
        private BF2Mod Mod;
        private BF2Mod LevelMod;

        private string _Name;
        private string BaseDir;

        private Dictionary<string, BF2Geometry> Geometries = new Dictionary<string, BF2Geometry>();
        private Dictionary<string, Texture> Textures = new Dictionary<string, Texture>(); // TODO
        public List<BF2Object> StaticObjects = new List<BF2Object>();

        public string Name
        {
            get { return _Name; }
        }

        public BF2Level(string baseDir)
        {
            this.BaseDir = baseDir;
            this._Name = new DirectoryInfo(baseDir).Name;
        }

        public void LoadStaticobjects()
        {
            string fileName = Path.Combine(this.BaseDir, "staticobjects.con");
            var lines = File.ReadAllLines(fileName);
            var text = File.ReadAllText(fileName);

            Regex newObjectRx = new Regex(@"^\s*Object.(create)\s+(?<Name>.*)$", RegexOptions.IgnoreCase);
            Regex editorRx = new Regex(@"^\s*if v_arg1 == BF2Editor$", RegexOptions.IgnoreCase);
            Regex endIfRx = new Regex(@"^\s*endIf$", RegexOptions.IgnoreCase);

            BF2Object activeSafe;
            bool skip = false;
            foreach (string line in lines)
            {
                if (line == "") { continue; }
                if (line.StartsWith("rem ")) { continue; }
                if (line.StartsWith("beginrem ")) { skip = true; continue; }
                if (line.StartsWith("endrem ")) { skip = false; continue; }

                // comments and skips first
                var editorMatch = editorRx.Match(line);
                if (editorMatch.Success)
                {
                    skip = true;
                    continue;
                }
                var endIfMatch = endIfRx.Match(line);
                if (endIfMatch.Success)
                {
                    skip = false;
                    continue;
                }

                if (skip) { continue; }
                // new objects, changing activeSafe
                var newMatch = newObjectRx.Match(line);
                if (newMatch.Success)
                {
                    string objectName = newMatch.Groups["Name"].Value.Trim();
                    BF2Object newObject = null;

                    // DEBUG
                    if (Mod.ObjectTemplates.ContainsKey(objectName) && this.LevelMod.ObjectTemplates.ContainsKey(objectName))
                    {
                        throw new NotImplementedException();
                    }
                    if (!Mod.ObjectTemplates.ContainsKey(objectName) && !this.LevelMod.ObjectTemplates.ContainsKey(objectName))
                    {
                        throw new NotImplementedException();
                    }

                    //
                    BF2Mod sourceMod;
                    if (this.LevelMod.ObjectTemplates.ContainsKey(objectName))
                    {

                        sourceMod = this.LevelMod;
                    }
                    else if (this.Mod.ObjectTemplates.ContainsKey(objectName))
                    {
                        sourceMod = this.Mod;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    BF2ObjectTemplate objectTemplate = sourceMod.ObjectTemplates[objectName];
                    newObject = objectTemplate.CreateObject();
                    if (objectTemplate.geometry != null) // object template has visible mesh
                    {
                        if (!this.Geometries.ContainsKey(objectTemplate.geometry)) // check if we dont have geometry loaded already
                        {
                            var loadedGeometry = newObject.LoadGeometry(sourceMod.GeometryTemplates[objectTemplate.geometry]);
                            this.Geometries.Add(objectTemplate.geometry, loadedGeometry);
                        }
                        else // otherwise assign already loaded mesh to object
                        {
                            newObject.Geometry = this.Geometries[objectTemplate.geometry];
                        }
                    }

                    this.StaticObjects.Add(newObject);
                    activeSafe = newObject;
                }
            }
        }

        public void Load(BF2Mod mod)
        {
            this.Mod = mod;
            this.LevelMod = new BF2Mod(this.BaseDir, true);
            this.LevelMod.LoadTemplates(this.LevelMod.OBJECTS_DIR);
            this.LoadStaticobjects();
        }
    }
}
