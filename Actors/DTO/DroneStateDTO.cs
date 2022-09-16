using Actors.MissionPathPriority;
using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.DTO
{
    public abstract class DroneStateDTO
    {
        public MissionPath Path { get; }
        public TimeSpan Age { get; }
        public IReadOnlySet<IActorRef> KnownNodes { get; }
        public IReadOnlySet<WaitingMissionDTO> ConflictSet { get; }
        public IReadOnlySet<FlyingMissionDTO> FlyingConflictMissions { get; }
        public int NegotiationsCount { get; } = 0;
        public Priority CurrentPriority { get; }

        protected DroneStateDTO(Mission thisMission, TimeSpan age, 
            IReadOnlySet<IActorRef> knownNodes, 
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions, 
            int negotiationsCount, Priority priority)
        {
            Path = thisMission.Path;
            Age = age;
            KnownNodes = knownNodes;

            ConflictSet = conflictSet.Select<WaitingMission, WaitingMissionDTO>(
                    mission => new WaitingMissionDTO(mission, mission.Priority))
                .ToHashSet();

            FlyingConflictMissions = flyingConflictMissions.Select<FlyingMission, FlyingMissionDTO>(
                    mission => new FlyingMissionDTO(mission, mission.GetRemainingTimeForSafeStart(thisMission)))
                .ToHashSet(); 


            NegotiationsCount = negotiationsCount;
            CurrentPriority = priority;
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

        public virtual TimeSpan TotalFlyTime() => TimeSpan.Zero;

        public virtual Point2D CurrentPosition() => Path.StartPoint;

        public virtual bool IsConnected() => true;

        public virtual bool IsFlying() => false;

        public virtual bool IsFinished() => false;
    }

    public class InitStateDTO : DroneStateDTO
    {

        public InitStateDTO(Mission thisMission, TimeSpan age, 
            IReadOnlySet<IActorRef> knownNodes, 
            IReadOnlySet<IActorRef> missingConnections, 
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions
            ) 
            : base(thisMission, age, knownNodes, 
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
        public NegotiateStateDTO(Mission thisMission, TimeSpan age, 
            IReadOnlySet<IActorRef> knownNodes, 
            IReadOnlySet<WaitingMission> conflictSet, 
            IReadOnlySet<FlyingMission> flyingConflictMissions, 
            int negotiationsCount, Priority lastCalculatedPriority) 
            : base(thisMission, age, knownNodes, 
                  conflictSet, flyingConflictMissions, 
                  negotiationsCount, lastCalculatedPriority)
        {
        }
    }

    public class WaitingStateDTO : DroneStateDTO
    {
        public WaitingStateDTO(Mission thisMission, TimeSpan age,
            IReadOnlySet<IActorRef> knownNodes,
            IReadOnlySet<WaitingMission> conflictSet,
            IReadOnlySet<FlyingMission> flyingConflictMissions,
            int negotiationsCount, Priority lastCalculatedPriority)
            : base(thisMission, age, knownNodes, 
                  conflictSet, flyingConflictMissions, 
                  negotiationsCount, lastCalculatedPriority)
        {
        }
    }

    public class FlyingStateDTO : DroneStateDTO 
    {

        private Point2D _currentPosition;
        private TimeSpan _flyTime;


        public TimeSpan ExtimatedRemainingFlyingTime { get; internal set; }

        public FlyingStateDTO(Mission thisMission, TimeSpan age, 
            IReadOnlySet<IActorRef> knownNodes, 
            int negotiationsCount, 
            Point2D currentPosition, 
            TimeSpan doneFlyingTime, 
            TimeSpan remainingFlyingTime
            ) 
            : base(thisMission, age, knownNodes, 
                  new HashSet<WaitingMission>(), new HashSet<FlyingMission>(), 
                  negotiationsCount, Priority.InfinitePriority)
        {
            _currentPosition = currentPosition;
            _flyTime = doneFlyingTime;
            ExtimatedRemainingFlyingTime = remainingFlyingTime;
        }

        public override bool IsFlying() => true;
        public override TimeSpan TotalFlyTime() => _flyTime;
        public override Point2D CurrentPosition() => _currentPosition;
    }

    public class EndStateDTO : DroneStateDTO
    {
        public EndStateDTO(Mission thisMission, TimeSpan age,
            IReadOnlySet<IActorRef> knownNodes,
            IReadOnlySet<WaitingMission> conflictSet,
            IReadOnlySet<FlyingMission> flyingConflictMissions,
            int negotiationsCount, Priority lastCalculatedPriority)
            : base(thisMission, age, knownNodes,
                  conflictSet, flyingConflictMissions,
                  negotiationsCount, lastCalculatedPriority)
        {
        }

        public override bool IsFinished() => false;
        public override Point2D CurrentPosition() => Path.StartPoint;
    }
}
