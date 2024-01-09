using CPU_Sim.SchematicEditor.Enum;
using CPU_Sim.SchematicEditor.Model;
using System.Windows;
using System.Windows.Controls;

namespace CPU_Sim.SchematicEditor.Class
{
    public class Bus<T>
    {
        public T Value { get; set; }
        public SignalModes Mode = 0; // 0=not defined, 1=bool(T=bool), 2=tri-State(T=int), 3=analog(T=float), 4=UserDefined

        //View:
        public List<Point> positions = new List<Point>();
      //  public List<Color> simulationColors = new List<Color>();
        public Item item = null;

        public Bus(T defaultValue, SignalModes mode, List<Point> positions, System.Windows.Media.Color defaultColor)
        {
            Value = defaultValue;
            Mode = mode;
            this.positions = positions;
            this.item = new Item(IDProvider.getIDProvider().getNewIDForBus(), "Wire", "Wire", defaultColor, System.Windows.Media.Colors.LightGray,true);
        }

        public void OnInputChange(List<Wire<T>> changedWires)
        {

        }
        public void OnShow(Canvas surface)
        {
        }
        public void Refresh()
        {

        }
        /*Info: Checks if any of the Postions are in this range
         * 
         */
        public bool isInRange(Point lowerRange, Point higherRange)
        {
            foreach (Point point in positions)
            {
                if (point.X < higherRange.X && point.X > lowerRange.X && point.Y < higherRange.Y && point.Y > lowerRange.Y)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
