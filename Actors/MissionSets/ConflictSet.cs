using Actors.MissionPathPriority;
using Akka.Actor;

namespace Actors.MissionSets
{
    /// <summary>
    /// Un set di missioni con cui sono in conflitto 
    /// (e presumibilmente sto negoziando).
    /// 
    /// In ogni momento posso utilizzarlo per estrarre insiemi
    /// di missione di priorità inferiore e superiore.
    /// </summary>
    public class ConflictSet : IMissionSet<WaitingMission>
    {
        /// <summary>
        /// Estrai tutte le missioni di priorità superiore a quella
        /// data in input.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public IDictionary<IActorRef, WaitingMission> GetGreaterPriorityMissions(Priority p)
        {
            return Missions.Where(pair => pair.Value.Priority.CompareTo(p) > 0).ToDictionary(
                mission => mission.Key,
                mission => mission.Value
                );
        }

        /// <summary>
        /// Estrai tutte le missioni di priorità inferiore a quella
        /// data in input.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public IDictionary<IActorRef, WaitingMission> GetSmallerPriorityMissions(Priority p)
        {
            return Missions.Where(pair => pair.Value.Priority.CompareTo(p) < 0).ToDictionary(
                mission => mission.Key,
                mission => mission.Value
                );
        }
        
        protected override WaitingMission CreateMission(IActorRef nodeRef, MissionPath path)
        {
            return new WaitingMission(nodeRef, path, Priority.InfinitePriority);
        }
    }


}
