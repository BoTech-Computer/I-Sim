using CPU_Sim.SchematicEditor.Model.ObjectData;
using CPU_Sim.SchematicEditor.Model;
using System.Windows;
using System.Text.Json;
using System.Windows.Media.Imaging;

using CPU_Sim.SchematicEditor.Model.Package;
using System.Linq;

namespace CPU_Sim.SchematicEditor.View.Menu
{
    /// <summary>
    /// Interaktionslogik für ObjectBrowser.xaml
    /// </summary>
    public partial class ObjectBrowser : Window
    {
        private List<PackageInI> externObjectsInI = new List<PackageInI>();
        private List<PackageInI> internObjectsInI = new List<PackageInI>();

        List<TableItem> TableItems = new List<TableItem>();
        private TableItem selectedObject = null;

        private bool ready = false;
        private int status = 0;

        private SchematicController controller = null;       

        public ObjectBrowser(SchematicController controller)
        {
            InitializeComponent();
            this.controller = controller;
        }


        public void ShowBrowser(List<PackageInI> internObjData, List<PackageInI> externObjData)//Returns the result of the dialog
        {
            this.Show();
            internObjectsInI = internObjData;
            externObjectsInI = externObjData;
            PrintObjects();
        }
  
        public int[] getSelectedObjectIndex() // returns the exact positions of in the array {ret[0] == 1 => externObject; ret[0] == 2 => internObject}{ret[1] => package Index}{ret[2]=> objectData Index}
        {
            int[] ret = new int[3];
            for(int p = 0;p < externObjectsInI.Count;p++)//For all externObjects
            {
                PackageInI package = externObjectsInI[p];
                for(int objData = 0; objData < package.ObjectData.Count; objData++)
                {
                    if(package.ObjectData[objData].cathegory.name == selectedObject.Cathegory && package.ObjectData[objData].Name == selectedObject.Name && package.packageName == selectedObject.Package && package.ObjectData[objData].ImageSource == selectedObject.ImageSourcePath && package.ObjectData[objData].Description == selectedObject.ObjectDescription)
                    {
                        ret[0] = 1;//extern Object
                        ret[1] = p;
                        ret[2] = objData;
                        return ret;
                    }
                }
            }
            for (int p = 0; p < internObjectsInI.Count; p++)//For all externObjects
            {
                PackageInI package = internObjectsInI[p];
                for (int objData = 0; objData < package.ObjectData.Count; objData++)
                {
                    if (package.ObjectData[objData].cathegory.name == selectedObject.Cathegory && package.ObjectData[objData].Name == selectedObject.Name && package.packageName == selectedObject.Package && package.ObjectData[objData].ImageSource == selectedObject.ImageSourcePath && package.ObjectData[objData].Description == selectedObject.ObjectDescription)
                    {
                        ret[0] = 2;//intern Object
                        ret[1] = p;
                        ret[2] = objData;
                        return ret;
                    }
                }
            }
            //not found
            ret[0] = -1;
            ret[1] = -1;
            ret[2] = -1;
            return ret;
        }
        private void PrintObjects()
        {
            List<PackageInI> VisibleSortetItems = new List<PackageInI>();
            foreach (PackageInI package in externObjectsInI)//For all externObjects
            {
                //Sort objectData for cathegories an print them on thge screen (ObjectTable)
                //get ObjectData
                //objectData => List of data of each dll
                List<ObjectData> objectData = package.ObjectData;

                List<Cathegory> cathegories = new List<Cathegory>();
                List<ObjectData> sortetItems = new List<ObjectData>();//Sorted Items that will be write back to the package
                cathegories.Add(objectData[0].cathegory);//First Cathegorey all other Cathegory are add in the foreach 
                foreach (ObjectData objData in objectData)
                {
                    if (!cathegories.Contains(objData.cathegory)) cathegories.Add(objData.cathegory);
                }
                foreach (Cathegory cathegory in cathegories)
                {
                    foreach (ObjectData objData in objectData)
                    {
                        if (cathegory.Equals(objData.cathegory)) sortetItems.Add(objData);
                    }
                }
                //Saving sortet items
                package.ObjectData = sortetItems;
                VisibleSortetItems.Add(package);
                
            }
            foreach (PackageInI package in internObjectsInI)//For all externObjects
            {
                //Sort objectData for cathegories an print them on thge screen (ObjectTable)
                //get ObjectData
                //objectData => List of data of each dll
                List<ObjectData> objectData = package.ObjectData;

                List<Cathegory> cathegories = new List<Cathegory>();
                List<ObjectData> sortetItems = new List<ObjectData>();
                cathegories.Add(objectData[0].cathegory);//First Cathegorey all other Cathegory are add in the foreach 
                foreach (ObjectData objData in objectData)
                {
                    if (!cathegories.Contains(objData.cathegory)) cathegories.Add(objData.cathegory);
                }
                foreach (Cathegory cathegory in cathegories)
                {
                    foreach (ObjectData objData in objectData)
                    {
                        if (cathegory.Equals(objData.cathegory)) sortetItems.Add(objData);
                    }
                }
                //Saving sortet items
                package.ObjectData = sortetItems;
                VisibleSortetItems.Add(package); 
            }
            
            //Creating readable strings
            foreach(PackageInI package in VisibleSortetItems)
            {
                foreach(ObjectData objData in package.ObjectData)
                {
                    TableItems.Add(new TableItem(objData.Name,"", objData.cathegory.name, package.packageName, objData.Description, objData.ImageSource,objData.cathegory.tags));
                }
                //List<string> buffer = new List<string>();
                 
                //VisibleStrings.Add(buffer);
            }
           // ObjectTable.Items.Clear();
            ObjectTable.ItemsSource = TableItems;
        }

        private void OK_BTN_Click(object sender, RoutedEventArgs e)
        { 
            if (selectedObject != null) { controller.AddObject(2, getSelectedObjectIndex()); this.Close(); } else MessageBox.Show("You do not have selected any Item. Please select on and click OK again", "Error:ObjectBrowser", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }

        private void Abort_BTN_Click(object sender, RoutedEventArgs e)
        {
            controller.AddObject(1, null);
            this.Close();
        }

        private void Help_BTN_Click(object sender, RoutedEventArgs e)
        {

        }
        private void SearchBTN_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "") // When the user Entered nothing! 
            {
                PrintObjects();
            }
            else
            {
                List<TableItem> tableItemsOld = TableItems;
                TableItems = new List<TableItem>();
                for (int i = 0; i < tableItemsOld.Count; i++)
                {
                    bool add = false; //when the object is added to the new list 
                                      //Cathegory tags:
                    for (int t = 0; t < tableItemsOld[i].Tags.Count; t++)
                    {
                        if (tableItemsOld[i].Tags[t].Contains(SearchBox.Text))
                        {
                            TableItems.Add(tableItemsOld[i]);
                            add = true;
                            break;
                        }
                    }
                    //Search in Object name
                    if (!add)
                    {
                        if (tableItemsOld[0].Name.Contains(SearchBox.Text))
                        {
                            TableItems.Add(tableItemsOld[i]);
                            add = true;
                            break;
                        }
                    }
                    //Searchz for cathegory name
                    if (!add)
                    {
                        if (tableItemsOld[0].Cathegory.Contains(SearchBox.Text))
                        {
                            TableItems.Add(tableItemsOld[i]);
                            add = true;
                            break;
                        }
                    }
                    //search in Object description
                    if (!add)
                    {
                        if (tableItemsOld[0].ObjectDescription.Contains(SearchBox.Text))
                        {
                            TableItems.Add(tableItemsOld[i]);
                            break;
                        }
                    }
                }
                ObjectTable.ItemsSource = TableItems;
            }
            
            
            
        }
        private void ObjectTable_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedObject = null;
            if (ObjectTable.SelectedItem != null)
            {
                if (typeof(TableItem).Equals(ObjectTable.SelectedItem.GetType()))
                {
                    TableItem objectData = (TableItem)ObjectTable.SelectedItem;
                    ObjectInfo.Text = objectData.ObjectDescription;
                    if (objectData.ImageSourcePath != null)
                    {
                        BitmapImage BitmapImage = new BitmapImage();
                        BitmapImage.BeginInit();
                        BitmapImage.UriSource = new Uri(objectData.ImageSourcePath);
                        ObjectImage.Source = BitmapImage;
                    }
                    selectedObject = objectData;
                }
            }
        }
        

        private class TableItem
        {
            public string Name { get; set; } = string.Empty;//ObjectName
            public string PackageDescription { get; set; } = string.Empty;//package.description
            public string Cathegory { get; set; } = string.Empty;//package.objectData[i].cathegory.name
            public string Package { get; set; } = string.Empty;//package.name
            public string ObjectDescription { get; set; } = string.Empty;//package.objectData[i].description
            public string ImageSourcePath { get; set; } = string.Empty;//
            public List<string> Tags {  get; set; } = new List<string>();

            public TableItem(string name, string packageDescription, string cathegory, string package, string objectDescription, string imageSourcePath, List<string> tags)
            {
                Name = name;//o
                PackageDescription = packageDescription;//o
                Cathegory = cathegory; //o
                Package = package; //o
                ObjectDescription = objectDescription;
                ImageSourcePath = imageSourcePath;
                Tags = tags;
            }
        }
    }
   
}
