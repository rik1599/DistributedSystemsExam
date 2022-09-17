using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.DroneStates
{
    /// <summary>
    /// Uno strumento per accedere dall'esterno agli stati 
    /// ed effettuare varie operazioni di lettura.
    /// 
    /// Utilizzare chiamando <code>state.PerformVisit(v);</code>
    /// sull'istanza della propria visita.
    /// 
    /// Implementare le visitie per ciascun stato ed estendere
    /// con gli opportuni getters per estrarre l'output.
    /// </summary>
    public abstract class DroneStateVisitor
    {
        internal abstract void Visit(InitState state);
        internal abstract void Visit(NegotiateState state);
        internal abstract void Visit(WaitingState state);
        internal abstract void Visit(FlyingState state);
        internal abstract void Visit(ExitState state);
    }
}
