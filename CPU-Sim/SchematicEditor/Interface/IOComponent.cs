using CPU_Sim.SchematicEditor.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*Info: This Interface is used to represend an object that can do something in the simulation and show its State.
 *      Duríng the Simulation the user can't do anything with it.
 *Example: LED, Display, Magnet...
 * 
 * 
 */
namespace CPU_Sim.SchematicEditor.Interface
{
    public interface IOComponent : Component
    {
        List<Graphic> SimComponents { get; }
    }
}
