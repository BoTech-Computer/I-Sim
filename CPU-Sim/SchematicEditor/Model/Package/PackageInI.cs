using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPU_Sim.SchematicEditor.Model.ObjectData;

namespace CPU_Sim.SchematicEditor.Model.Package
{
    public class PackageInI
    {
        public string packagePath = string.Empty;//stores the dll path that have to be imported
        public List<CPU_Sim.SchematicEditor.Model.ObjectData.ObjectData> ObjectData = new List<CPU_Sim.SchematicEditor.Model.ObjectData.ObjectData>();// stores data of the importet classes such as name of the class 
        public string packageName = string.Empty;
        public string Author = string.Empty;

        public PackageInI(string packagePath, List<ObjectData.ObjectData> objectData, string packageName, string author)
        {
            this.packagePath = packagePath;
            ObjectData = objectData;
            this.packageName = packageName;
            Author = author;
        }
    }
}
