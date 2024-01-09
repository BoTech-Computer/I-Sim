using CPU_Sim.SchematicEditor.Model.Package;

namespace CPU_Sim.SchematicEditor.Model.Project
{
    public class ProjectInI
    {
        public string Name { get; set; } = string.Empty;
        public string mainSchematic { get; set; } = string.Empty; // complete json paths
        public List<string> schematicPaths { get; set; } = new List<string>(); // path index in this list is equals to the List subSchematics in SchematicController.
        public List<PackageInI> usedPackages { get; set; } = new List<PackageInI>();

    }
}
