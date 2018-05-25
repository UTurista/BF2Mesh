using BF2Mesh;
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
    public class BF2Mod
    {
        public readonly string Name;
        public readonly string modPath;
        public readonly string LEVELS_DIR;
        public readonly string OBJECTS_DIR;

        public readonly Dictionary<string, BF2ObjectTemplate> ObjectTemplates = new Dictionary<string, BF2ObjectTemplate>();
        public readonly Dictionary<string, BF2GeometryTemplate> GeometryTemplates = new Dictionary<string, BF2GeometryTemplate>();
        // TODO: collisions
        public readonly Dictionary<string, BF2Level> Levels = new Dictionary<string, BF2Level>();

        public BF2Mod(string modDir, bool isLevel = false)
        {
            this.modPath = modDir;

            if (!isLevel)
            {
                this.Name = new DirectoryInfo(this.modPath).Name;

                this.LEVELS_DIR = Path.Combine(this.modPath, "levels");

                LoadLevels(this.LEVELS_DIR);
            }

            this.OBJECTS_DIR = Path.Combine(this.modPath, "objects");
            LoadTemplates(this.OBJECTS_DIR);
        }

        public void LoadTemplates(string templatesPath)
        {
            if (!Directory.Exists(templatesPath))
            {
                //throw new NotImplementedException();
                return;
            }
            IEnumerable<string> confiles = Directory.EnumerateFiles(templatesPath, "*.con", SearchOption.AllDirectories);
            foreach (string fileName in confiles)
            {
                var conFile = new ConFile().ParseConFile(fileName);
                foreach (BF2ObjectTemplate objectTemplate in conFile.ObjectTemplates)
                {
                    if (!this.ObjectTemplates.ContainsKey(objectTemplate.Name))
                    {
                        this.ObjectTemplates.Add(objectTemplate.Name, objectTemplate);
                    }
                    else
                    {
                        this.ObjectTemplates[objectTemplate.Name].Update(objectTemplate);
                    }
                }


                foreach (BF2GeometryTemplate geometryTemplate in conFile.GeometryTemplates)
                {
                    if (!GeometryTemplates.ContainsKey(geometryTemplate.Name))
                    {
                        GeometryTemplates.Add(geometryTemplate.Name, geometryTemplate);
                    }
                    else
                    {
                        GeometryTemplates[geometryTemplate.Name].Update(geometryTemplate);
                    }
                }
            }
        }

        private void LoadLevels(string levelsPath)
        {
            foreach (string folderName in Directory.EnumerateDirectories(levelsPath))
            {
                if (!File.Exists(Path.Combine(folderName, "init.con"))) { continue; }

                string levelName = new DirectoryInfo(folderName).Name;
                this.Levels.Add(levelName, new BF2Level(folderName));
            }
        }
    }
}
