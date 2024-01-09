using CPU_Sim.SchematicEditor.Class;
using CPU_Sim.SchematicEditor.Enum;
using CPU_Sim.SchematicEditor.Interface;
using CPU_Sim.SchematicEditor.Model;
using CPU_Sim.SchematicEditor.Model.ObjectData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace InBuiltObjects.Logic.Gates
{
    public class Inverter : Component
    {
        public List<Pin<bool>> pinsBool { get; set; }
        public List<Pin<int>> pinsTriState {  get; set; }
        public List<Pin<float>> pinsAnalog {  get; set; }

        public Item item {  get; set; }
        public ObjectData objectData { get; set; }

        public Inverter(Point position) 
        {
            pinsAnalog = null;
            pinsTriState = null;
            pinsBool =
            [
                new Pin<bool>(false, SignalModesPins.BoolInput, new Point(position.X - 25, position.Y), System.Windows.Media.Colors.GreenYellow),//Input
                new Pin<bool>(false, SignalModesPins.BoolOutput, new Point(position.X + 25, position.Y), System.Windows.Media.Colors.GreenYellow),//Output
            ];
            item = new Item(IDProvider.getIDProvider().getNewIDForComponent(), "Inverter-(InBuildObjects_Inverter)", "Inverter", System.Windows.Media.Colors.Green, System.Windows.Media.Colors.Black, true, false, position);
        }
        public void OnDelet()
        {
            IDProvider.getIDProvider().DeleteIDForComponent(item.ID);//Delete this ID
        }
        public void OnShow(Canvas surface) 
        { 
            foreach(var pin in pinsBool)
            {
                pin.OnShow(surface);
            }
            item.OnShow(surface);
            if (item.Visible)
            {
                Point position = item.position;
                //Triangle:
                Line line = new Line();
                line.X1 = position.X - (3 * item.zoom);
                line.Y1 = position.Y + (4 * item.zoom);
                line.X2 = position.X - (3 * item.zoom);
                line.Y2 = position.Y - (4 * item.zoom);
                line.Stroke = new SolidColorBrush(item.lineColor);
                Line line2 = new Line();
                line.X1 = position.X - (3 * item.zoom);
                line.Y1 = position.Y + (4 * item.zoom);
                line.X2 = position.X + (3 * item.zoom);
                line.Y2 = position.Y;
                line.Stroke = new SolidColorBrush(item.lineColor);
                Line line3 = new Line();
                line.X1 = position.X - (3 * item.zoom);
                line.Y1 = position.Y - (4 * item.zoom);
                line.X2 = position.X + (3 * item.zoom);
                line.Y2 = position.Y;
                line.Stroke = new SolidColorBrush(item.lineColor);
                surface.Children.Add(line);
                surface.Children.Add(line2);
                surface.Children.Add(line3);
            }
        }
        public void OnInputChange()
        {
            Behavior();
        }
        
        public void Refresh() 
        {
            Behavior();
            //OnShow();
        }
        private void Behavior()
        {
            if (pinsBool[0].Value) pinsBool[1].Value = true;
            else pinsBool[1].Value = false;
        }
    }
}
