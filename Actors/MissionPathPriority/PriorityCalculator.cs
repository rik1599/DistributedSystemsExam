using Akka.Actor;
using System.Diagnostics;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Strumento per il calcolo della priorità di un nodo 
    /// </summary>
    public class PriorityCalculator
    {
        private const float k = 1.2f;
        
        /// <summary>
        /// Calcola la priorità di un certo nodo dato il suo ID, 
        /// il suo conflict set e il suo flying set.
        /// </summary>
        /// <param name="nodeRef">Il mio riferimento</param>
        /// <param name="mission"></param>
        /// <param name="age"></param>
        /// <param name="conflictSet">I nodi con cui sto negoziando</param>
        /// <param name="flyingSet">I nodi in volo di cui sto attendendo il termine</param>
        /// <param name="nodeRef"></param>
        /// <param name="conflictSet"></param>
        /// <param name="flyingSet"></param>
        /// <returns></returns>
        public static Priority CalculatePriority(IActorRef nodeRef, Mission mission, TimeSpan age, Mission[] conflictSet, FlyingMission[] flyingSet)
        {
            // calcolo del massimo tempo di attesa di missioni in volo
            Double maxFlyingMissionsWait = 0f;
            foreach(var m in flyingSet)
            {
                var remainingTime = ParseValue(m.GetRemainingTimeForSafeStart(mission));
                if (remainingTime > maxFlyingMissionsWait)
                {
                    maxFlyingMissionsWait = remainingTime;
                }
            }

            // calcolo della somma dei tempi che faccio attendere il mio conflict set
            Double sumOfWaitsICause = 0f;
            foreach (var m in conflictSet)
            {
                var conflictPoint = m.Path.ClosestConflictPoint(mission.Path);

                Debug.Assert(conflictPoint != null);

                sumOfWaitsICause += ParseValue(
                    // timeDist(mission.start, conflictPoint) - 
                    //  timeDist(m.start, conflictPoint) 
                    mission.Path.TimeDistance(conflictPoint.Value).Subtract(
                        m.Path.TimeDistance(conflictPoint.Value)
                        ));
            }

            return new Priority(
                Math.Pow(ParseValue(age), k) - maxFlyingMissionsWait - sumOfWaitsICause,
                nodeRef
                );
        }


        private static Double ParseValue(TimeSpan time)
        {
            // TODO: gestisci meglio l'estrazione dell'unità di misura corretta
            return (double) time.TotalMinutes;
        }
    }
}
