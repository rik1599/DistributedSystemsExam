using Actors.MissionPathPriority;
using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.DTO
{
    public class DroneStateDTO
    {
        
        public DateTime DroneTimestamp { get; } = DateTime.Now;

        public IActorRef NodeRef { get; }

        public MissionPath Path { get; }
        public TimeSpan Age { get; }
        public IReadOnlySet<IActorRef> KnownNodes { get; }
        public IReadOnlySet<WaitingMissionDTO> ConflictSet { get; }
        public IReadOnlySet<FlyingMissionDTO> FlyingConflictMissions { get; }
        public int NegotiationsCount { get; } = 0;
        public Priority CurrentPriority { get; }

        internal DroneStateDTO(DroneActorContext droneContext, 
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions, 
            int negotiationsCount, Priority priority)
        {
            Path = droneContext.ThisMission.Path;
            Age = droneContext.Age;
            KnownNodes = droneContext.Nodes.ToHashSet();

            ConflictSet = conflictSet.Select<WaitingMission, WaitingMissionDTO>(
                    mission => new WaitingMissionDTO(mission, mission.Priority))
                .ToHashSet();

            FlyingConflictMissions = flyingConflictMissions.Select<FlyingMission, FlyingMissionDTO>(
                    mission => new FlyingMissionDTO(mission, mission.GetRemainingTimeForSafeStart(droneContext.ThisMission)))
                .ToHashSet(); 


            NegotiationsCount = negotiationsCount;
            CurrentPriority = priority;

            NodeRef = droneContext.Context.Self;
        }

        /// <summary>
        /// Le missioni con priorità maggiore della mia
        /// </summary>
        /// <returns></returns>
        public IReadOnlySet<WaitingMissionDTO> GetGreaterPriorityMissions() 
        { 
            return ConflictSet.Where(mission => mission.Priority.CompareTo(CurrentPriority) > 0).ToHashSet();
        }

        /// <summary>
        /// Le missioni con priorità minore della mia
        /// </summary>
        /// <returns></returns>
        public IReadOnlySet<WaitingMissionDTO> GetSmallerPriorityMissions()
        {
            return ConflictSet.Where(mission => mission.Priority.CompareTo(CurrentPriority) < 0).ToHashSet();
        }

        public virtual TimeSpan TotalWaitTime() => Age;

        public virtual TimeSpan DoneFlyTime() => TimeSpan.Zero;

        public virtual Point2D CurrentPosition() => Path.StartPoint;

        public virtual bool IsConnected() => true;

        public virtual bool IsFlying() => false;

        public virtual bool IsMissionAccomplished() => false;
    }

    public class InitStateDTO : DroneStateDTO
    {

        internal InitStateDTO(DroneActorContext droneContext, 
            IReadOnlySet<IActorRef> missingConnections, 
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions
            ) 
            : base(droneContext, 
                  conflictSet, flyingConflictMissions, 
                  0, Priority.NullPriority)
        {
            MissingConnections = missingConnections;
        }

        public IReadOnlySet<IActorRef> MissingConnections { get; internal set; }

        public override bool IsConnected() => false;
    }

    public class NegotiateStateDTO
        : DroneStateDTO
    {
        internal NegotiateStateDTO(DroneActorContext droneContext,
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions, 
            int negotiationsCount, Priority lastCalculatedPriority) 
            : base(droneContext, 
                  conflictSet, flyingConflictMissions, 
                  negotiationsCount, lastCalculatedPriority)
        {
        }
    }

    public class WaitingStateDTO : DroneStateDTO
    {
        internal WaitingStateDTO(DroneActorContext droneContext,
            IReadOnlySet<WaitingMission> conflictSet,
            IReadOnlySet<FlyingMission> flyingConflictMissions,
            int negotiationsCount, Priority lastCalculatedPriority)
            : base(droneContext,
                  conflictSet, flyingConflictMissions, 
                  negotiationsCount, lastCalculatedPriority)
        {
        }
    }

    public class FlyingStateDTO : DroneStateDTO 
    {

        private readonly Point2D _currentPosition;
        private readonly TimeSpan _flyTime;

        public TimeSpan ExtimatedRemainingFlyingTime { get; internal set; }

        internal FlyingStateDTO(DroneActorContext droneContext,
            int negotiationsCount, 
            Point2D currentPosition, 
            TimeSpan doneFlyingTime, 
            TimeSpan remainingFlyingTime
            ) 
            : base(droneContext, 
                  new HashSet<WaitingMission>(), new HashSet<FlyingMission>(), 
                  negotiationsCount, Priority.InfinitePriority)
        {
            _currentPosition = currentPosition;
            _flyTime = doneFlyingTime;
            ExtimatedRemainingFlyingTime = remainingFlyingTime;
        }

        public override bool IsFlying() => true;
        public override TimeSpan DoneFlyTime() => _flyTime;
        public override Point2D CurrentPosition() => _currentPosition;
    }

    public class ExitStateDTO : DroneStateDTO
    {
        private readonly bool _isMissionAccomplished;
        
        public DroneStateDTO PrecedentState { get; }

        public string Motivation { get; }

        internal ExitStateDTO(DroneActorContext droneContext,
            int negotiationsCount,
            bool isMissionAccomplished, 
            string motivation, 
            DroneStateDTO precedentState)
            : base(droneContext,
                  new HashSet<WaitingMission>(), new HashSet<FlyingMission>(),
                  negotiationsCount, Priority.NullPriority)
        {
            _isMissionAccomplished = isMissionAccomplished;
            PrecedentState = precedentState;
            Motivation = motivation;
        }

        public override bool IsMissionAccomplished() => _isMissionAccomplished;
        public override Point2D CurrentPosition() => Path.StartPoint;
    }
}
