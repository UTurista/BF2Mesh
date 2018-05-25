using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BF2Mesh;
using System.IO;
using BF2MeshConsole.Templates;

namespace BF2MeshConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string PR_REPO_DIR = System.Environment.GetEnvironmentVariable("PR_REPO");
            string LEVEL = "saaremaa";

            var mod = new BF2Mod(PR_REPO_DIR);

            var level = mod.Levels[LEVEL];
            level.Load(mod);

            Dictionary<string, List<BF2Object>> usedTextures = new Dictionary<string, List<BF2Object>>();
            foreach (BF2Object staticObject in level.StaticObjects)
            {
                if (staticObject.Geometry == null) { continue; }
                foreach (string texture in staticObject.Geometry.GetTextures())
                {
                    string fixedTexture = texture;
                    if (texture[0] != '/')
                    {
                        fixedTexture = "/" + texture;
                    }
                    if (!usedTextures.ContainsKey(fixedTexture))
                    {
                        usedTextures.Add(fixedTexture, new List<BF2Object>());
                    }
                    usedTextures[fixedTexture].Add(staticObject);
                }
            }

            // TODO: refactor output
            string outputLogPath = Path.Combine(PR_REPO_DIR, "textureUsage_" + LEVEL + ".log");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputLogPath))
            {
                var sortedTextures = from entry in usedTextures orderby entry.Value.Count descending select entry;
                foreach (var usedTexture in sortedTextures)
                {
                    string usedCount = usedTexture.Value.Count.ToString();
                    string logMessage = string.Format("{0}: {1}", usedCount, usedTexture.Key);
                    file.WriteLine(logMessage);

                    List<BF2Object> objects = usedTexture.Value;
                    List<string> wroteObjects = new List<string>();
                    foreach (var bf2object in objects)
                    {
                        if (wroteObjects.Contains(bf2object.ObjectTemplate.Name)) { continue; }
                        wroteObjects.Add(bf2object.ObjectTemplate.Name);

                        // too lazy to remember linq
                        List<BF2Object> sameObjects = new List<BF2Object>();
                        foreach (var obj in objects)
                        {
                            if (obj.ObjectTemplate.Name == bf2object.ObjectTemplate.Name)
                            {
                                sameObjects.Add(obj);
                            }
                        }
                        string objectCount = string.Format("[{0}]", sameObjects.Count);
                        file.WriteLine(bf2object.ObjectTemplate.Name + " " + objectCount);
                    }

                    file.WriteLine("");
                }
            }
        }
    }
}
