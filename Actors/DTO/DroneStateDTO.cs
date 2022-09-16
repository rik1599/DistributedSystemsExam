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

        public virtual bool IsConnected() => true;

        public virtual bool IsFlying() => false;
    }

    public class DroneInitStateDTO : DroneStateDTO
    {

        public DroneInitStateDTO(Mission thisMission, TimeSpan age, 
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

    public class DroneNegotiateStateDTO : DroneStateDTO
    {
        public DroneNegotiateStateDTO(Mission thisMission, TimeSpan age, 
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

    public class DroneWaitingStateDTO : DroneStateDTO
    {
        public DroneWaitingStateDTO(Mission thisMission, TimeSpan age,
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

    public class DroneFlyingStateDTO : DroneStateDTO 
    {
        public Point2D CurrentPosition { get; internal set; }
        public TimeSpan DoneFlyingTime { get; internal set; }
        public TimeSpan RemainingFlyingTime { get; internal set; }

        public override bool IsFlying() => true;

        public DroneFlyingStateDTO(Mission thisMission, TimeSpan age, 
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
            CurrentPosition = currentPosition;
            DoneFlyingTime = doneFlyingTime;
            RemainingFlyingTime = remainingFlyingTime;
        }
    }
}
