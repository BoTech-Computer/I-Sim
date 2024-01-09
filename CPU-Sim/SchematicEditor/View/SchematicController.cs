using CPU_Sim.SchematicEditor.Class;
using CPU_Sim.SchematicEditor.Enum.Error;
using CPU_Sim.SchematicEditor.Model.Package;
using CPU_Sim.SchematicEditor.View.Menu;
using System.IO;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;
using CPU_Sim.SchematicEditor.Model.Project;
using CPU_Sim.SchematicEditor.Model.ObjectData;
using CPU_Sim.SchematicEditor.Interface;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Shapes;
using System.Windows.Media;

namespace CPU_Sim.SchematicEditor.View
{
    /* Important Paths:
     * 1. ProgrammData/BoTech/CPU-Sim/SchematicEditor/packages/packagesInI.json => Store all dll File Names
     * 2. ProgrammData/BoTech/CPU-Sim/SchematicEditor/packages/{Package_Name}/ObjectData.json => List<ObjectData> 
     */

    public class SchematicController
    {
        private Canvas surface = null;// Main Surface of the view

        private ProjectInI projectInI = null;
        private string projectPath = string.Empty; // stores the exact path of the project ini file.

        private Schematic MainSchematic = new Schematic();
        private object ViewSchematic = new Schematic(); //Schematic whgich the user is working on
        public List<Point> ViewField = new List<Point>();//Holds only two points which reprersents the shown field. 
        private Point oldMousePosition = new Point(); // Saves the Mouse Position after calculating
        private List<SubSchematic> subSchematics = new List<SubSchematic>();
        private int zoom = 16;//16 is the default value


        //Selection Rect
        private bool SelectionActive = false;
        private Point FirstPosition = new Point();
        private Point LastPosition = new Point();
        private List<object> selectedObjects = new List<object>();

        private List<Assembly> externObjects = new List<Assembly>();
        private List<Assembly> internObjects = new List<Assembly>();
        private List<PackageInI> externObjectsInI = new List<PackageInI>();
        private List<PackageInI> internObjectsInI = new List<PackageInI>();

        //View Flags
        private object MoveAroundObject = null;
        private bool MovingNewObject = false;
        private int[] MoveAroundObjectPackageRef = new int[3];// returns the exact positions of in the array {ret[0] == 1 => externObject; ret[0] == 2 => internObject}{ret[1]

        private bool MovingSchematic = false;//When the user click the middle mouse button

        private string ErrorLogInit = string.Empty;// Represents the a list of all Errors that have been occured durting the intitialisation
        /******************************************************-INIT-Procedures-******************************************************
         * Info: DataPath => include the base ProgrammData Directory of this Application e.g.(C:/ProgramData/BoTech/CPU-Sim/...)
         *       The returned error represents only the last occurred Error. FOr further information use the Atrribute ErrorLogInit.
         */
        public Error Init(string DataPath)
        {
            FileError ret = FileError.None;
            if((ret = LoadInternAssemblys(DataPath)) == FileError.None)
            {
                if((ret = LoadExternAssemblys(DataPath)) == FileError.None)
                {
                    MessageBox.Show("Loading Ok load all Dll's", "Status");
                }
                else
                {
                    ErrorLogInit = ErrorLogInit + "\nError: { \n   Name: Can not load the extern Assemblys! \n   Error Args: " + ret.ToString() + "\n}END";
                }
            }
            else
            {
                ErrorLogInit = ErrorLogInit + "\nError: { \n   Name: Can not load the intern Assemblys! \n   Error Args: " + ret.ToString() + "\n}END";
            }
            return (Error)ret;
        }
        /* Info: Load intern/extern Assermblys for the Simulation and View (objects)
         */
        private FileError LoadExternAssemblys(string DataPath)
        {
            if (File.Exists(DataPath + "SchematicEditor\\packages\\extern\\packageInI.json"))
            {
                externObjectsInI = null;
                try
                {
                    externObjectsInI = JsonConvert.DeserializeObject<List<PackageInI>>(File.ReadAllText(DataPath + "SchematicEditor\\packages\\extern\\packageInI.json"));
                }
                catch
                {
                    return FileError.JsonDeserializeObjectError;
                }
                if (externObjectsInI != null)
                {
                    foreach (PackageInI packageInI in externObjectsInI)
                    {
                        if (packageInI != null)
                        {
                            if (packageInI.packagePath != string.Empty)
                            {
                                internObjects.Add(Assembly.LoadFile(packageInI.packagePath));
                            }
                            else return FileError.ErrorByLoadingAssembly;
                        }
                        else return FileError.ResultObjectSetToNull;
                    }
                }
                else return FileError.ResultObjectSetToNull;
            }
            else return FileError.FileNotFound;
            return FileError.None;
        }
        private FileError LoadInternAssemblys(string DataPath)
        {
            if (File.Exists(DataPath + "SchematicEditor\\packages\\intern\\packageInI.json"))
            {
                internObjectsInI = null;
                try
                {
                    internObjectsInI = JsonConvert.DeserializeObject<List<PackageInI>>(File.ReadAllText(DataPath + "SchematicEditor\\packages\\intern\\packageInI.json"));
                }
                catch 
                {
                    return FileError.JsonDeserializeObjectError;
                }
                if (internObjectsInI != null)
                {
                    foreach (PackageInI packageInI in internObjectsInI)
                    {
                        if (packageInI != null)
                        {
                            if (packageInI.packagePath != string.Empty)
                            {
                                internObjects.Add(Assembly.LoadFile(packageInI.packagePath));
                            }
                            else return FileError.ErrorByLoadingAssembly;
                        }
                        else return FileError.ResultObjectSetToNull;
                    }
                }
                else return FileError.ResultObjectSetToNull;
            }
            else return FileError.FileNotFound;

            return FileError.None;
        }
        public void setSurface(Canvas surface)
        {
            this.surface = surface;
        }

        public void NewProject(string rootPath, string ProjectName)
        {
            projectInI = new ProjectInI();
            projectInI.Name = ProjectName;
            projectInI.mainSchematic = rootPath + "\\MainSchematic\\" + projectInI.Name + ".json";
            Directory.CreateDirectory(rootPath + "\\MainSchematic\\");
            //File.Create(rootPath + "\\" + projectInI.Name + ".json");//Create Project File
            //File.Create(projectInI.mainSchematic);//Create Main Schematic File
            File.WriteAllText(rootPath + "\\" + projectInI.Name + ".json", JsonConvert.SerializeObject(projectInI)); // Safe this ProjectInI
            MainSchematic = new Schematic();
            File.WriteAllText(projectInI.mainSchematic, JsonConvert.SerializeObject(MainSchematic));  
        }
        public FileError LoadProject(string ProjectPath)
        {
            if (File.Exists(ProjectPath))
            {
                try
                {
                    projectInI = JsonConvert.DeserializeObject<ProjectInI>(File.ReadAllText(ProjectPath));
                    if (projectInI != null)
                    {
                        MainSchematic = JsonConvert.DeserializeObject<Schematic>(File.ReadAllText(projectInI.mainSchematic)); // Load the Main schematic
                        if (MainSchematic != null)
                        {
                            ViewSchematic = MainSchematic;
                            foreach (string path in projectInI.schematicPaths)
                            {
                                subSchematics.Add(JsonConvert.DeserializeObject<SubSchematic>(File.ReadAllText(path)));
                                if (subSchematics[subSchematics.Count - 1] == null)
                                {
                                    return FileError.JsonDeserializeObjectError;
                                }
                            }
                            return FileError.None;
                        }
                        else return FileError.ResultObjectSetToNull;
                    }
                    else return FileError.ResultObjectSetToNull;
                }
                catch
                {
                    return FileError.JsonDeserializeObjectError;
                }
            }
            return FileError.None;
        }
        public FileError SaveProject(string projectPath)
        {
            if (File.Exists(projectPath))
            {
                string json = JsonConvert.SerializeObject(projectInI);
                if (json != null)
                {
                    MainSchematic = (Schematic)ViewSchematic;
                    File.WriteAllText(projectPath, json);
                    json = JsonConvert.SerializeObject(MainSchematic);
                    if (json != null)
                    {
                        File.WriteAllText(projectInI.mainSchematic, json);
                        for (int i = 0; i < subSchematics.Count; i++)
                        {
                            json = JsonConvert.SerializeObject(subSchematics[i]);
                            if (json != null)
                            {
                                File.WriteAllText(projectInI.schematicPaths[i], json);
                            }
                            else return FileError.JsonSeserializeObjectError;
                        }
                    }
                    else return FileError.JsonSeserializeObjectError;
                }
                else return FileError.JsonSeserializeObjectError;
            }
            return FileError.None;
        }
        /*Info: newPath includes only the new Directory. The Project name have to be entered in the Attribute ProjectNAme without File extension
         */
        public FileError SaveProjectAs(string newPath, string ProjectName)
        {
            string oldProjectRootPath = RemoveMatches(projectPath, projectInI.Name + ".json");//Project root path without {PorjectName}.json
            projectInI.Name = ProjectName;
            projectInI.mainSchematic = newPath + "\\MainSchematic\\" + projectInI.Name + ".json";
            Directory.CreateDirectory(newPath + "\\MainSchematic\\");
            File.Create(newPath + "\\" + projectInI.Name + ".json");//Create Project File
            File.Create(projectInI.mainSchematic);//Create Main Schematic File



            for (int i = 0;i < projectInI.schematicPaths.Count;i++)
            {
                projectInI.schematicPaths[i] = newPath + RemoveMatches(projectInI.schematicPaths[i], oldProjectRootPath); // receives the directory in which the schematic was located. The old project root path is exchanged for the new project root path.
                Directory.CreateDirectory(RemoveMatches(projectInI.schematicPaths[i], subSchematics[i].Name + ".json")); // keep's only the directory path to the file
                File.Create(projectInI.schematicPaths[i]);
            }
            return SaveProject(newPath + "\\" + projectInI.Name);//Real save routine
        }
        public string getProjectName()
        {
            return projectInI.Name;
        }

        private string RemoveMatches(string s1, string s2)// Source: https://stackoverflow.com/questions/55035602/deleting-a-string-from-another-string-c-sharp
        {
            //Console.WriteLine(s1);
            for (int i = Math.Max(0, s1.Length - s2.Length); i < s1.Length; i++)
            {
                var ss1 = s1.Substring(i, s1.Length - i);
                var ss2 = s2.Substring(0, Math.Min(s2.Length, ss1.Length));
                //Console.WriteLine(ss2.PadLeft(s1.Length));
                if (ss1 == ss2)
                    return s1.Substring(0, i);
            }

            return s1;
        }
        /******************************************************-WORKING-PROCEDURES-******************************************************/
        /**************************-MOUSE-EVENTS-**************************/
        /*Info: MouseEvents that have to be controlled
         *      1. Right-Click => Schow Menu
         *      2. Left-Click => Prepare for Move; Select Object; MoveAround;
         *      3. Middle-Click => Move Schematic Around if mouse postion changed
         *      4. Mouse-Move => Move ONLY new objects around
         */
        public bool RightClick(Point mousePosition, bool state)
        {
            return false; 
        }
        public bool LeftClick(Point mousePosition, bool state)
        {
            //Storing the new Object into the schematic object
            if (MovingNewObject && state)
            {
                //Convert Object and store in the right list
                if (MoveAroundObjectPackageRef[0] == 1)//extern Object
                {
                    //Converting the object to the given type and draw it on the surface
                    switch (externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Type)
                    {
                        case 0: MessageBox.Show("Package Error: Object Type not set: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); break;
                        case 1:
                            try
                            {
                                ((Component)MoveAroundObject).item.position = mousePosition;
                                if (typeof(Schematic) == ViewSchematic.GetType())
                                {
                                    ((Schematic)ViewSchematic).AddComponent((Component)MoveAroundObject);
                                }
                                else
                                {
                                    ((SubSchematic)ViewSchematic).AddComponent((Component)MoveAroundObject);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        case 2:
                            try
                            {
                                ((IOComponent)MoveAroundObject).item.position = mousePosition;

                                if (typeof(Schematic) == ViewSchematic.GetType())
                                {
                                    ((Schematic)ViewSchematic).AddComponent((IOComponent)MoveAroundObject);
                                }
                                else
                                {
                                    ((SubSchematic)ViewSchematic).AddComponent((IOComponent)MoveAroundObject);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        case 3:
                            try
                            {
                                ((IOUserInterface)MoveAroundObject).item.position = mousePosition;
                                if (typeof(Schematic) == ViewSchematic.GetType())
                                {
                                    ((Schematic)ViewSchematic).AddComponent((IOUserInterface)MoveAroundObject);
                                }
                                else
                                {
                                    ((SubSchematic)ViewSchematic).AddComponent((IOUserInterface)MoveAroundObject);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                    }
                }
                else
                {
                    if (MoveAroundObjectPackageRef[0] == 2)//intern Object
                    {
                        switch (internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Type)
                        {
                            case 0: MessageBox.Show("Package Error: Object Type not set: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); break;
                            case 1:
                                try
                                {
                                    Point actualPosition = new Point();
                                    actualPosition.X = (ViewField[1].X - surface.ActualWidth) + mousePosition.X;
                                    actualPosition.Y = (ViewField[1].Y - surface.ActualHeight) + mousePosition.Y;

                                    ((Component)MoveAroundObject).item.position = actualPosition;
                                    if (typeof(Schematic) == ViewSchematic.GetType())
                                    {
                                        ((Schematic)ViewSchematic).AddComponent((Component)MoveAroundObject);
                                        ((Schematic)ViewSchematic).ShowAll(ViewField[0], ViewField[1], surface, zoom);
                                        MessageBox.Show("Placed Item to Position : " + actualPosition.ToString() + "; View Field1: " + ViewField[0].ToString() + "; View Field2: " + ViewField[1].ToString() + "; ActualWidth: " + surface.ActualWidth + "; ActualHeight: " + surface.ActualHeight, "Info");
                                    }
                                    else
                                    {
                                        ((SubSchematic)ViewSchematic).AddComponent((Component)MoveAroundObject);
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                            case 2:
                                try
                                {
                                    ((IOComponent)MoveAroundObject).item.position = mousePosition;
                                    if (typeof(Schematic) == ViewSchematic.GetType())
                                    {
                                        ((Schematic)ViewSchematic).AddComponent((IOComponent)MoveAroundObject);
                                    }
                                    else
                                    {
                                        ((SubSchematic)ViewSchematic).AddComponent((IOComponent)MoveAroundObject);
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                            case 3:
                                try
                                {
                                    ((IOUserInterface)MoveAroundObject).item.position = mousePosition;
                                    if (typeof(Schematic) == ViewSchematic.GetType())
                                    {
                                        ((Schematic)ViewSchematic).AddComponent((IOUserInterface)MoveAroundObject);
                                    }
                                    else
                                    {
                                        ((SubSchematic)ViewSchematic).AddComponent((IOUserInterface)MoveAroundObject);
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                        }
                    }
                }
                MoveAroundObject = null;//Deleting object
                MovingNewObject = false;//Now object should be move
                MoveAroundObjectPackageRef = new int[3];//Deleting references
                ShowAll(ViewField[0], ViewField[1]);
                return true;//something had changed
            }
            else
            {
                if (state) { SelectObjects(mousePosition); } else { SelectionActive = false; LastPosition = mousePosition; }
            }
           
            return false;//nothing had changed
        }
        public bool MiddleClick(Point mousePosition, bool state)
        {
            if (!MovingNewObject)
            { //When the user press the middle Button during 
                MovingSchematic = state;
                return true;//something had changed
            }
            return false;
        }
        /*Info: returns true if the view have changed
         */
        public bool MouseMove(Point mousePosition)
        {
            if (MovingNewObject)
            {
                MoveSchematic(mousePosition);
                //Geting the Object Type
                if (MoveAroundObjectPackageRef[0] == 1)//extern Object
                {
                    //Converting the object to the given type and draw it on the surface
                    switch (externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Type)
                    {
                        case 0: MessageBox.Show("Package Error: Object Type not set: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);break;
                        case 1: 
                            try 
                            {
                                ((Component)MoveAroundObject).item.position = mousePosition;
                                ShowAll(ViewField[0], ViewField[1]);
                                return true;//something had changed
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        break;
                        case 2:
                            try
                            {
                                ((IOComponent)MoveAroundObject).item.position = mousePosition;
                                ShowAll(ViewField[0], ViewField[1]);
                                return true;//something had changed
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        case 3:
                            try
                            {
                                ((IOUserInterface)MoveAroundObject).item.position = mousePosition;
                                ShowAll(ViewField[0], ViewField[1]);
                                return true;//something had changed
                            }
                            catch
                            {
                                MessageBox.Show("Package Error: Cannot convert Object to Component: " + externObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + externObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                    }
                }
                else
                {
                    if (MoveAroundObjectPackageRef[0] == 2)//intern Object
                    {
                        switch (internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Type)
                        {
                            case 0: MessageBox.Show("Package Error: Object Type not set: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); break;
                            case 1:
                                try
                                {
                                    ShowAll(ViewField[0], ViewField[1]);
                                    ((Component)MoveAroundObject).item.position = mousePosition;
                                    ((Component)MoveAroundObject).item.Visible = true;
                                    ((Component)MoveAroundObject).OnShow(surface);
                                   // 
                                    return true;//something had changed
                                }
                                
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                            case 2:
                                try
                                {
                                    ShowAll(ViewField[0], ViewField[1]);
                                    ((IOComponent)MoveAroundObject).item.position = mousePosition;
                                    ((IOComponent)MoveAroundObject).item.Visible = true;
                                    ((IOComponent)MoveAroundObject).OnShow(surface);
                                    return true;//something had changed
                                }
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                            case 3:
                                try
                                {
                                    ShowAll(ViewField[0], ViewField[1]);
                                    ((IOUserInterface)MoveAroundObject).item.position = mousePosition;
                                    ((IOUserInterface)MoveAroundObject).item.Visible = true;
                                    ((IOUserInterface)MoveAroundObject).OnShow(surface); 
                                    return true;//something had changed
                                }
                                catch
                                {
                                    MessageBox.Show("Package Error: Cannot convert Object to Component: " + internObjectsInI[MoveAroundObjectPackageRef[1]].packageName + ", Class: " + internObjectsInI[MoveAroundObjectPackageRef[1]].ObjectData[MoveAroundObjectPackageRef[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                break;
                        }
                    }
                }
            }
            if (MovingSchematic)
            {
                MoveSchematic(mousePosition);
                ShowAll(ViewField[0], ViewField[1]);
                return true;//something had changed
            }
            return false;//nothing had changed

        }
        public void setZoom(int zoom)
        {
            this.zoom = zoom;
            ShowAll(ViewField[0], ViewField[1]);
        }

        private void SelectObjects(Point mousePosition)
        {
            if (ViewSchematic.GetType() == typeof(Schematic))
            {
                //surface.Children.Clear();//clearing screen
               
                selectedObjects = new List<object>();
                if ((selectedObjects = ((Schematic)ViewSchematic).getSelectedObjects(mousePosition, 16)) == null)
                {

                }
                else// nothing selected => Selection Rect has to show
                {
                    if (!SelectionActive)
                    {
                        SelectionActive = true;
                        FirstPosition = mousePosition;
                    }
                    /*Rectangle SelectRectangle = new Rectangle();    
                    SelectRectangle.SetValue(Canvas.TopProperty, mousePosition.Y);
                    SelectRectangle.SetValue(Canvas.LeftProperty, mousePosition.X);
                    SelectRectangle.SetValue(Canvas.RightProperty, FirstPosition.X);
                    SelectRectangle.SetValue(Canvas.BottomProperty, FirstPosition.Y);
                    SelectRectangle.Stroke = new SolidColorBrush(Colors.Blue);
                    */
                    RectangleGeometry rectangleGeometry = new RectangleGeometry();
                    if ((mousePosition.X - FirstPosition.X) > 0 && (mousePosition.Y - FirstPosition.Y) > 0)
                    {
                        rectangleGeometry.Rect = new Rect(FirstPosition.X, FirstPosition.Y, mousePosition.X - FirstPosition.X, mousePosition.Y - FirstPosition.Y);

                        System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                        path.Fill = Brushes.Transparent;
                        path.Stroke = Brushes.Black;
                        path.StrokeThickness = 1;
                        path.Data = rectangleGeometry;

                        surface.Children.Add(path);
                    }
                    if ((selectedObjects = ((Schematic)ViewSchematic).getSelectedObjects(oldMousePosition, mousePosition)) == null)
                    {

                    }
                }
            }
            else
            {
                if (ViewSchematic.GetType() == typeof(SubSchematic))
                {
                   
                }
            }
        }
        private void MoveSchematic(Point currentMousePosition)
        {
            if (ViewField.Count == 0)//When set to null => when schematic editor starts
            {
                ViewField.Add(new Point(0, 0));
                ViewField.Add(new Point(surface.ActualWidth, surface.ActualHeight));
            }
            else
            {
                Point lowerRange = ViewField[0];
                Point higherRange = ViewField[1];
                if (lowerRange == null) //set to null when sized of the view changed
                {
                    lowerRange = new Point(0, 0);
                }
                if (higherRange == null)
                {
                    higherRange = new Point(surface.ActualWidth, surface.ActualHeight); // Max view size
                }
                lowerRange.X = lowerRange.X + (currentMousePosition.X - oldMousePosition.X);// Calculates the distance dX=new.X-old.X; dY=new.Y-old.Y;
                lowerRange.Y = lowerRange.Y + (currentMousePosition.Y - oldMousePosition.Y);
                higherRange.X = higherRange.X + (currentMousePosition.X - oldMousePosition.X);// Calculates the distance dX=new.X-old.X; dY=new.Y-old.Y;
                higherRange.Y = higherRange.Y + (currentMousePosition.Y - oldMousePosition.Y);
                ViewField[0] = lowerRange;
                ViewField[1] = higherRange;

            }
        }
        private void ShowAll(Point lowerRange, Point higherRange)
        {
            if(ViewSchematic.GetType() == typeof(Schematic))
            {
                //surface.Children.Clear();//clearing screen
                ((Schematic)ViewSchematic).ShowAll(lowerRange, higherRange, surface, zoom);
            }
            else
            {
                if(ViewSchematic.GetType() == typeof(SubSchematic))
                {
                    //surface.Children.Clear();
                    ((SubSchematic)ViewSchematic).ShowAll(lowerRange, higherRange, surface);
                }
            }
        }
        private void addToSchematic(Component component)
        {
            
        }

        public void AddObject()
        {
            ObjectBrowser browser = new ObjectBrowser(this);
            browser.ShowBrowser(internObjectsInI, externObjectsInI);
        }
        /*Info: Function that the ObjectBrowser call. Use the Function AddObject() to give the User the option wwhich object he want to place. 
         */
        public void AddObject(int DialogResult, int[] ret)
        {

            if (DialogResult == 2)
            {
                if (ret[0] == 1)//Extern Object
                {
                    Type classType = externObjects[ret[1]].GetType(externObjectsInI[ret[1]].ObjectData[ret[2]].Name);
                    MoveAroundObject = Activator.CreateInstance(classType,new Point(0,0)); //gets the class name and creates an instance with the default position
                    if(MoveAroundObject == null)
                    {
                        MessageBox.Show("Error by creating an instance of: Package:" + externObjectsInI[ret[1]].packageName + ", Class: " + externObjectsInI[ret[1]].ObjectData[ret[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        MovingNewObject = false;
                    }
                    else
                    {
                        MoveAroundObjectPackageRef = ret;
                        MovingNewObject = true;
                    }

                }
                else//Intern Object 
                {
                    if (ret[0] == 2)
                    {
                        Type classType = internObjects[ret[1]].GetType(internObjectsInI[ret[1]].ObjectData[ret[2]].Name);
                        MoveAroundObject = Activator.CreateInstance(classType, new Point(0, 0)); //gets the class name and creates an instance with the default position
                        if (MoveAroundObject == null)
                        {
                            MessageBox.Show("Error by creating an instance of: Package:" + internObjectsInI[ret[1]].packageName + ", Class: " + internObjectsInI[ret[1]].ObjectData[ret[2]].Name + ". When this error occours a second time, reinstall this Package or call the BoTech service.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            MovingNewObject = false;
                        }
                        else
                        {
                            MoveAroundObjectPackageRef = ret;
                            MovingNewObject = true;
                        }
                    }
                }
                //MessageBox.Show("Ok: " + ret[0].ToString() + ret[1].ToString() + ret[2].ToString());
            }
            else if (DialogResult == 1)
            {
                MessageBox.Show("cancel");
            }
        }
    }
}
 