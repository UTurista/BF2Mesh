using BF2MeshConsole.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BF2MeshConsole
{
    public class ConFile
    {
        public string FileName;

        private Dictionary<string, BF2ObjectTemplate> _ObjectTemplates = new Dictionary<string, BF2ObjectTemplate>();
        private Dictionary<string, BF2GeometryTemplate> _GeometryTemplates = new Dictionary<string, BF2GeometryTemplate>();

        public List<BF2ObjectTemplate> ObjectTemplates
        {
            get { return this._ObjectTemplates.Values.ToList(); }
        }

        public List<BF2GeometryTemplate> GeometryTemplates
        {
            get { return this._GeometryTemplates.Values.ToList(); }
        }

        public ConFile ParseConFile(string fileName)
        {
            this.FileName = fileName;

            Regex newObjectTemplateRx = new Regex(@"^\s*ObjectTemplate.(create|activeSafe)\s+(?<Type>.*?)\s+(?<Name>.*)$", RegexOptions.IgnoreCase);
            Regex newGeometryTemplateRx = new Regex(@"^\s*GeometryTemplate.(create)\s+(?<Type>.*?)\s+(?<Name>.*)$", RegexOptions.IgnoreCase);
            Regex propertyRx = new Regex(@"^\s*ObjectTemplate\.(?<Property>.*?)\s+(?<Value>.*)$", RegexOptions.IgnoreCase);
            Regex includeRx = new Regex(@"^\s*(include|run)\s+(?<Path>.*)$", RegexOptions.IgnoreCase);

            var lines = File.ReadAllLines(fileName);
            BF2ObjectTemplate activeSafe = null;
            bool skip;
            List<string> missingFields = new List<string>();
            foreach (string line in lines)
            {
                if (line == "") { continue; }
                if (line.StartsWith("rem ")) { continue; }
                if (line.StartsWith("beginrem ")) { skip = true; continue; }
                if (line.StartsWith("endrem ")) { skip = false; continue; }

                // matches
                var newObjectTemplateMatch = newObjectTemplateRx.Match(line);
                var newGeometryTemplateMatch = newGeometryTemplateRx.Match(line);
                var propMatch = propertyRx.Match(line);
                var inclMatch = includeRx.Match(line);

                if (newObjectTemplateMatch.Success)
                {
                    string objectName = newObjectTemplateMatch.Groups["Name"].Value.Trim();
                    if (!this._ObjectTemplates.ContainsKey(objectName))
                    {
                        this._ObjectTemplates.Add(objectName, new BF2ObjectTemplate(objectName));
                    }
                    activeSafe = this._ObjectTemplates[objectName];
                    continue;
                }

                if (newGeometryTemplateMatch.Success)
                {
                    string geometryType = newGeometryTemplateMatch.Groups["Type"].Value.Trim();
                    MeshType meshType;
                    bool meshTypeParsed = Enum.TryParse(geometryType, true, out meshType);
                    if (!meshTypeParsed) { throw new NotImplementedException(); }
                    string geometryName = newGeometryTemplateMatch.Groups["Name"].Value.Trim();
                    if (!this._GeometryTemplates.ContainsKey(geometryName))
                    {
                        string meshFileName = Path.Combine(
                                                Path.GetDirectoryName(fileName),
                                                "meshes",
                                                geometryName);
                        var geometryTemplate = new BF2GeometryTemplate(meshFileName, meshType);
                        if (geometryTemplate.FileName != null)
                        {
                            this._GeometryTemplates.Add(geometryName, geometryTemplate);
                        }
                    }
                    continue;
                }

                if (propMatch.Success)
                {
                    var property = propMatch.Groups["Property"].Value.Trim();
                    var value = propMatch.Groups["Value"].Value.Trim();

                    FieldInfo field = activeSafe.GetType().GetField(property);
                    if (field == null)
                    {
                        missingFields.Add(property);
                    }
                    else
                    {
                        field.SetValue(activeSafe, Convert.ChangeType(value, field.FieldType));
                    }
                }
            }

            return this;
        }
    }
}
