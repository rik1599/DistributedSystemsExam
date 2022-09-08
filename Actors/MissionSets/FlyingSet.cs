using Actors.MissionPathPriority;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.MissionSets
{
    /// <summary>
    /// Strumento per la gestione delle missioni in volo. 
    /// 
    /// Quando aggiungo una missione, si registra automaticamente
    /// la data e l'ora corrente istante della partenza.
    /// </summary>
    public class FlyingSet : IMissionSet<FlyingMission>
    {
        protected override FlyingMission CreateMission(IActorRef nodeRef, MissionPath path)
        {
            // generazione 
            return new FlyingMission(nodeRef, path, DateTime.Now);
        }
    }
}
