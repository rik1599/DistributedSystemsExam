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
        internal abstract void Visit(DroneActorState state);
        internal virtual void Visit(InitState state) { Visit((DroneActorState) state); }
        internal virtual void Visit(NegotiateState state) { Visit((DroneActorState) state); }
        internal virtual void Visit(WaitingState state) { Visit((DroneActorState) state); }
        internal virtual void Visit(FlyingState state) { Visit((DroneActorState) state); }
        internal virtual void Visit(ExitState state) { Visit((DroneActorState) state); }
    }
}
