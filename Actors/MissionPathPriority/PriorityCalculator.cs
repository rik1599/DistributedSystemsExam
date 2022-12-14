using System.Diagnostics;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Strumento per il calcolo della priorità di un nodo.
    /// </summary>
    internal static class PriorityCalculator
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
            var maxFlyingMissionsWait = flyingSet.Aggregate(
                TimeSpan.Zero,
                (partialMax, mission) =>
                {
                    var remainingTime = mission.GetRemainingTimeForSafeStart(thisMission);
                    return remainingTime > partialMax ? remainingTime : partialMax;
                });
            
            // calcolo della somma dei tempi che faccio attendere il mio conflict set
            TimeSpan sumOfWaitsICause = conflictSet.Aggregate(
                TimeSpan.Zero,
                (partialSum, mission) =>
                {
                    var conflictPoint = mission.Path.ClosestConflictPoint(thisMission.Path);
                    Debug.Assert(conflictPoint != null);
                    return partialSum + thisMission.Path.TimeDistance(conflictPoint.Value) - mission.Path.TimeDistance(conflictPoint.Value);
                });

            return new Priority(
                Math.Pow(ParseValue(age), k)
                    - h1 * conflictSet.Count * ParseValue(maxFlyingMissionsWait)
                    - h2 * ParseValue(sumOfWaitsICause),
                thisMission.NodeRef
            );
        }


        private static double ParseValue(TimeSpan time)
        {
            // TODO: gestisci meglio l'estrazione dell'unità di misura corretta
            return (double)time.TotalMinutes;
        }
    }
}
