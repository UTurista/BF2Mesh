using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BF2Mesh;
using System.Reflection;

namespace BF2MeshConsole.Templates
{
    public class BF2ObjectTemplate
    {
        private string _Name;

        // common properties
        public string geometry;
        public int saveInSeparateFile;
        public int PreCacheObject;

        public string Name
        {
            get { return _Name; }
        }

        public BF2ObjectTemplate(string name)
        {
            this._Name = name;
        }

        public BF2Object CreateObject()
        {
            var newObject = new BF2Object(this);
            return newObject;
        }

        public bool Update(BF2ObjectTemplate objectTemplate)
        {
            foreach (FieldInfo field in objectTemplate.GetType().GetFields())
            {
                var value = field.GetValue(objectTemplate);
                field.SetValue(this, Convert.ChangeType(value, field.FieldType));
            }

            return true;
        }
    }
}
