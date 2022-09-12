using Akka.Actor;
using System.Diagnostics;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Strumento per il calcolo della priorità di un nodo.
    /// </summary>
    public class PriorityCalculator
    {
        private const float k = 1.2f;
        private const float h1 = 1f;
        private const float h2 = 1f;
        
        /// <summary>
        /// Calcola la priorità di una certa missione.
        /// </summary>
        /// <param name="thisMission">La missione di cui voglio calcolare la priorità</param>
        /// <param name="age">Il tempo che una missione ha già atteso</param>
        /// <param name="conflictSet">I nodi con cui sto negoziando</param>
        /// <param name="flyingSet">I nodi in volo di cui sto attendendo il termine</param>
        /// <returns></returns>
        public static Priority CalculatePriority(Mission thisMission, TimeSpan age, ISet<WaitingMission> conflictSet, ISet<FlyingMission> flyingSet)
        {
            // calcolo del massimo tempo di attesa di missioni in volo
            var maxFlyingMissionsWait = TimeSpan.Zero;
            foreach(var m in flyingSet)
            {
                var remainingTime = m.GetRemainingTimeForSafeStart(thisMission);
                if (remainingTime > maxFlyingMissionsWait)
                {
                    maxFlyingMissionsWait = remainingTime;
                }
            }

            // calcolo della somma dei tempi che faccio attendere il mio conflict set
            var sumOfWaitsICause = TimeSpan.Zero;
            foreach (var m in conflictSet)
            {
                var conflictPoint = m.Path.ClosestConflictPoint(thisMission.Path);

                Debug.Assert(conflictPoint != null);

                // comporto ad ogni nodo un tempo di attesa almeno tale
                // alla differenza tra il mio tempo per raggiungere il
                // punto di conflitto e il suo tempo

                // NOTA: può essere anche un valore negativo, e in tal 
                // caso mi fa guadagnare priorità.

                sumOfWaitsICause = sumOfWaitsICause.Add(

                    // timeDist(thisMission.start, conflictPoint) - 
                    //  timeDist(m.start, conflictPoint) 
                    thisMission.Path.TimeDistance(conflictPoint.Value)
                        .Subtract(m.Path.TimeDistance(conflictPoint.Value))
                );
            }

            return new Priority(
                Math.Pow(ParseValue(age), k) 
                    - h1 * conflictSet.Count * ParseValue(maxFlyingMissionsWait) 
                    - h2 * ParseValue(sumOfWaitsICause),
                thisMission.NodeRef
            );
        }


        private static Double ParseValue(TimeSpan time)
        {
            // TODO: gestisci meglio l'estrazione dell'unità di misura corretta
            return (double) time.TotalMinutes;
        }
    }
}
