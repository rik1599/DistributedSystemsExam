using Akka.Actor;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Strumento per il calcolo della priorità di un nodo 
    /// </summary>
    public class PriorityCalculator
    {
        /// <summary>
        /// Calcola la priorità di un certo nodo dato il suo ID, 
        /// il suo conflict set e il suo flying set.
        /// </summary>
        /// <param name="nodeRef">Il mio riferimento</param>
        /// <param name="conflictSet">I nodi con cui sto negoziando</param>
        /// <param name="flyingSet">I nodi in volo di cui sto attendendo il termine</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Priority CalculatePriority(IActorRef nodeRef, Mission[] conflictSet, FlyingMission[] flyingSet)
        {
            throw new NotImplementedException();
        }
    }
}
