using Akka.Actor;
using MathNet.Spatial.Euclidean;
using System.Diagnostics;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Missione. E' caratterizzata dalla sua tratta (utilizzabile
    /// per calcolare conflitti) e da un riferimento al nodo 
    /// che la porta a termine.
    /// 
    /// E' una struttura immutabile.
    /// </summary>
    public abstract class Mission
    {
        public IActorRef NodeRef { get; private set; }

        public MissionPath Path { get; private set; }

        protected Mission(IActorRef nodeRef, MissionPath path)
        {
            NodeRef = nodeRef;
            Path = path;
        }
    }

    /// <summary>
    /// Missione in attesa di partire. E' caratterizzata da una priorità (modificabile).
    /// </summary>
    public class WaitingMission : Mission
    {
        public Priority Priority { get; set; }

        public WaitingMission(IActorRef nodeRef, MissionPath path, Priority priority) : base(nodeRef, path)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// Missione in volo. 
    /// 
    /// IN LOCALE, può essere utilizzata per calcolare:
    /// - il tempo rimanente per il completamento
    /// - il tempo rimanente per il raggiungimento di un certo punto
    /// - il tempo che una certa missione (in conflitto) deve attendere
    ///     per effettuare una partenza in sicurezza.
    ///     
    /// ATTENZIONE, se si trasferisce questo oggetto su un altro
    /// attore, assicurarsi che gli orologi siano sincronizzati per 
    /// fare i calcoli.
    /// </summary>
    public class FlyingMission : Mission
    {
        private readonly DateTime _startTime;

        public FlyingMission(IActorRef nodeRef, MissionPath path, DateTime startTime) : base(nodeRef, path)
        {
            _startTime = startTime;
        }

        /// <summary>
        /// Tempo rimanente per il completamento.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TimeSpan GetRemainingTime()
        {
            return GetRemainingTimeToPoint(Path.EndPoint);
        }

        /// <summary>
        /// Tempo che la missione ci metterà per raggiungere una certa posizione.
        /// Ritorna un tempo nullo se il punto è già stato raggiunto e 
        /// un'eccezione se il punto non si trova sulla tratta. 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TimeSpan GetRemainingTimeToPoint(Point2D p)
        {
            // il punto deve appartenere alla tratta (entro un margine)
            Debug.Assert(Path.PathSegment.LineTo(p).Length <= MissionPath.MarginDistance);
            
            // timeDist(start, p) - [now - startTime]
            return Path.TimeDistance(p).Subtract(DateTime.Now.Subtract(_startTime));
        }

        /// <summary>
        /// Tempo rimanente che LA MISSIONE PASSATA IN INPUT deve 
        /// attendere per garantire che la partenza sia sicura.
        /// </summary>
        /// <param name="thisMission">
        /// La missione in attesa (cioè la missione del nodo corrente)
        /// </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TimeSpan GetRemainingTimeForSafeStart(Mission thisMission)
        {
            Point2D? conflictPoint = thisMission.Path.ClosestConflictPoint(Path);

            if (conflictPoint == null) return TimeSpan.Zero;

            var timeDistFlyPoint = Path.TimeDistance(conflictPoint.Value);
            var timeDistWaitPoint = thisMission.Path.TimeDistance(conflictPoint.Value);
            var alreadyWaitedTime = DateTime.Now.Subtract(_startTime);
            var marginTime = new TimeSpan(0, 0, (int)(MissionPath.MarginDistance / thisMission.Path.Speed));

            var safeWaitTime = timeDistFlyPoint - timeDistWaitPoint - alreadyWaitedTime + marginTime;

            // timeDist(start, p) - timeDist(thisMission.start, p) - [now - startTime] + margin
            return safeWaitTime;
        }
    }
}
