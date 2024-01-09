using CPU_Sim.SchematicEditor.Model.ObjectData;
using CPU_Sim.SchematicEditor.Model.Package;
using CPU_Sim.SchematicEditor.Enum.Error;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using CPU_Sim.SchematicEditor.Class;


namespace CPU_Sim.SchematicEditor.View
{
    /// <summary>
    /// Interaktionslogik für SchematicEditor.xaml
    /// </summary>
    public partial class SchematicEditor : Window
    {

        private bool SurfaceViewChanged = true;
        private int SurfaceZoom = 16;
        private int MaxSurfaceZoom = 128;
        private Point MousePosition = new Point(0, 0);
        private Point PagePosition = new Point(0, 0);
        private SchematicController schematicController = new SchematicController();

        private string ProjectPath = string.Empty;
        private bool alreadySaved = false;  

        public SchematicEditor()
        {
            InitializeComponent();
        }
    
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializingView();
            /*List<PackageInI> packageInIs = new List<PackageInI>();
            List<ObjectData> objectDatas = new List<ObjectData>();
            objectDatas.Add(new ObjectData("And", "Normal And Gate behavior with two inputs.", new Cathegory("Gates", "", new List<string>())));
            objectDatas.Add(new ObjectData("Inverter", "Normal Inverter behavior.", new Cathegory("Gates", "", new List<string>())));
            packageInIs.Add(new PackageInI("C:\\ProgramData\\BoTech\\CPU-Sim\\SchematicEditor\\packages\\intern\\InBuiltObjects.dll",objectDatas,"InBuild","BoTech-Dervcelopment"));

            string t = JsonConvert.SerializeObject(packageInIs);
            //File.Create("C:\\ProgramData\\BoTech\\CPU-Sim\\SchematicEditor\\packages\\intern\\packageInI.json");
            
            File.WriteAllText("C:\\ProgramData\\BoTech\\CPU-Sim\\SchematicEditor\\packages\\intern\\packageInI.json", t);
            */
            schematicController = new SchematicController();
            schematicController.Init("C:\\ProgramData\\BoTech\\CPU-Sim\\");
            schematicController.setSurface(Surface);
            string id = IDProvider.getIDProvider().getNewIDForBus();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshSurface();
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && SurfaceZoom < MaxSurfaceZoom) { SurfaceZoom++; } else { if (e.Delta < 0 && SurfaceZoom > 8) { SurfaceZoom--; } }
            RefreshSurface();
            schematicController.setZoom(SurfaceZoom);
        }
        private void Surface_MouseEnter(object sender, MouseEventArgs e)
        {
        /*    if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                //Distance between the old mouse Postion and the new Mousposition
                PagePosition.X = PagePosition.X + (e.GetPosition(Surface).X - MousePosition.X);
                PagePosition.Y = PagePosition.Y + (e.GetPosition(Surface).Y - MousePosition.Y);
                RefreshSurface();
            }
            MousePosition = e.GetPosition(Surface);
            bool result = schematicController.MouseMove(MousePosition);
            if(e.LeftButton.Equals(MouseButtonState.Pressed)) 
            { 
                schematicController.LeftClick(e.GetPosition(Surface), true);  
            } 
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.MiddleClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.MiddleClick(e.GetPosition(Surface) ,false); }
            }
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.RightClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.RightClick(e.GetPosition(Surface), false); }
            }

            if(result)
            {
                DrawBackground(SurfaceZoom, (int)Surface.ActualWidth, (int)Surface.ActualHeight);
            }

            SufaceCursorData.Content = "Zoom: " + SurfaceZoom + ", Pos: X:" + (int)MousePosition.X + "Y:" + (int)MousePosition.Y;
        */
        }
        private void Surface_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                //Distance between the old mouse Postion and the new Mousposition
                PagePosition.X = PagePosition.X + (e.GetPosition(Surface).X - MousePosition.X);
                PagePosition.Y = PagePosition.Y + (e.GetPosition(Surface).Y - MousePosition.Y);
                
            }
            RefreshSurface();
            MousePosition = e.GetPosition(Surface);
            bool result = schematicController.MouseMove(MousePosition);
            if (e.LeftButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.LeftClick(e.GetPosition(Surface), true);
            }
            else
            {
                schematicController.LeftClick(e.GetPosition(Surface), false);
            }
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.MiddleClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.MiddleClick(e.GetPosition(Surface), false); }
            }
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.RightClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.RightClick(e.GetPosition(Surface), false); }
            }

            if (result)
            {
                DrawBackground(SurfaceZoom, (int)Surface.ActualWidth, (int)Surface.ActualHeight);
            }

            if(schematicController.ViewField.Count > 0)SufaceCursorData.Content = "Zoom: " + SurfaceZoom + ", Pos: X:" + (int)MousePosition.X + "Y:" + (int)MousePosition.Y + "ViewField: " + schematicController.ViewField[0].ToString() + "; ViewField2: " + schematicController.ViewField[1].ToString();
        }
        private void Surface_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                //Distance between the old mouse Position and the new Mouseposition
                PagePosition.X = PagePosition.X + (e.GetPosition(Surface).X - MousePosition.X);
                PagePosition.Y = PagePosition.Y + (e.GetPosition(Surface).Y - MousePosition.Y);
               
            }
            RefreshSurface();
            MousePosition = e.GetPosition(Surface);
            bool result = schematicController.MouseMove(MousePosition);
            if (e.LeftButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.LeftClick(e.GetPosition(Surface), true);
            }
            else
            {
                schematicController.LeftClick(e.GetPosition(Surface), false);
            }
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.MiddleClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.MiddleClick(e.GetPosition(Surface), false); }
            }
            if (e.MiddleButton.Equals(MouseButtonState.Pressed))
            {
                schematicController.RightClick(e.GetPosition(Surface), true);
            }
            else
            {
                if (e.MiddleButton.Equals(MouseButtonState.Released)) { schematicController.RightClick(e.GetPosition(Surface), false); }
            }

            if (result)
            {
                DrawBackground(SurfaceZoom, (int)Surface.ActualWidth, (int)Surface.ActualHeight);
            }


            if (schematicController.ViewField.Count > 0) SufaceCursorData.Content = "Zoom: " + SurfaceZoom + ", Pos: X:" + (int)MousePosition.X + "Y:" + (int)MousePosition.Y + "ViewField: " + schematicController.ViewField[0].ToString() + "; ViewField2: " + schematicController.ViewField[1].ToString();
        }
        /*****************************-BUTTON-EVENTS-*****************************/
        private void NewObjectBTN_Click(object sender, RoutedEventArgs e)
        {
            schematicController.AddObject(); 
        }

        private void NewSchematicMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewSimulationMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddProjectMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSchematicMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSimSettingsMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSubSchematicMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewObjectMenuBTN_Click(object sender, RoutedEventArgs e)
        {

        }
        //Project Operations
        private void NewProjectMenuBTN_Click(object sender, RoutedEventArgs e)
        {
            schematicController = new SchematicController();
            schematicController.Init("C:\\ProgramData\\BoTech\\CPU-Sim\\");
            var dialog = new SaveFileDialog();
            dialog.Title = "Save your Project";
            dialog.Filter = @"Project File|*.json";
            if (dialog.ShowDialog() == true)
            {
                schematicController.NewProject(RemoveMatches(dialog.FileName, dialog.SafeFileName), RemoveMatches(dialog.SafeFileName, ".json"));
                ProjectPath = dialog.FileName;
                alreadySaved = false;
                this.Title = "SchematicEditor: " + schematicController.getProjectName();
            }
        }
        private void OpenMenuBTN_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Open your Project";
            dialog.Filter = @"Project File|*.json";
            if (dialog.ShowDialog() == true)
            {
                schematicController = new SchematicController();
                schematicController.Init("C:\\ProgramData\\BoTech\\CPU-Sim\\");
                if (schematicController.LoadProject(dialog.FileName) == FileError.None)
                { // Complete Path with file extension and name
                    ProjectPath = dialog.FileName;
                    alreadySaved = true;
                    this.Title = "SchematicEditor: " + schematicController.getProjectName();
                }
            }
        }
        private void SaveMenuBTN_Click(object sender, RoutedEventArgs e)
        {
            if (alreadySaved)
            {
                schematicController.SaveProject(ProjectPath);
            }
            else
            {
                var dialog = new SaveFileDialog();
                dialog.Title = "Save your Project";
                dialog.Filter = @"Project File|*.json";
                if (dialog.ShowDialog() == true)
                {
                    schematicController.SaveProjectAs(RemoveMatches(dialog.FileName, dialog.SafeFileName), dialog.SafeFileName);
                    ProjectPath = dialog.FileName;
                    alreadySaved = true;
                }
            }
        }
        private void SaveAsMenuBTN_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "Save your Project";
            dialog.Filter = @"Project File|*.json";
            if (dialog.ShowDialog() == true)
            {
                schematicController.SaveProjectAs(RemoveMatches(dialog.FileName, dialog.SafeFileName), dialog.SafeFileName);
                ProjectPath = dialog.FileName;
                alreadySaved = true;
                this.Title = "SchematicEditor: " + schematicController.getProjectName();
            }
        }

        private void CloseMenuBTN_Click(object sender, RoutedEventArgs e)
        {
            if(!alreadySaved)
            {
                MessageBox.Show("Do you want to save?", "Information", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            }
            this.Close();
        }


        private void InitializingView()
        {
            RefreshSurface();

        }
        private void RefreshSurface()
        {
            Surface.Children.Clear();
            SufaceCursorData.Content = "Zoom: " + SurfaceZoom + ", Pos: X:" + (int)MousePosition.X + "Y:" + (int)MousePosition.Y;
            DrawBackground(SurfaceZoom, (int)Surface.ActualWidth, (int)Surface.ActualHeight);
        }
        private void DrawBackground(int spacing, int maxX, int maxY)
        {
            MainStatusLBL.Content = "Drawing Background...";
            if (maxX < 0) maxX = maxX * -1;
            if (maxY < 0) maxY = maxY * -1;

            Rectangle rectangle = new Rectangle();
            rectangle.Stroke = new SolidColorBrush(Colors.Blue);
            rectangle.Width = maxX;
            rectangle.Height = maxY;
            Surface.Children.Add(rectangle);

            for (int x = 0; x < (maxX / spacing) * spacing; x = x + spacing)
            { 
                Line line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Gray);
                line.X1 = x;
                line.Y1 = 0;
                line.X2 = x;
                line.Y2 = maxY;

                Surface.Children.Add(line);
            }
            for (int y = 0; y < (maxY / spacing) * spacing; y = y + spacing)
            {
                Line line = new Line();
                line.Stroke = new SolidColorBrush(Colors.Gray);
                line.X1 = 0;
                line.Y1 = y;
                line.X2 = maxX;
                line.Y2 = y;

                Surface.Children.Add(line);
            }
            MainStatusLBL.Content = "Ready";
        }

        //String Operations
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


    }
}