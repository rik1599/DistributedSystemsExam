using Actors.MissionPathPriority;
using Akka.Actor;

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
