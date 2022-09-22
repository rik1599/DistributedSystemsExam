using Actors.MissionPathPriority;
using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.DTO
{
    public class DroneStateDTO
    {
        public DateTime DroneTimestamp { get; }
        public IActorRef NodeRef { get; }
        public MissionPath Path { get; }
        public TimeSpan Age { get; }
        public IReadOnlySet<IActorRef> KnownNodes { get; }
        public IReadOnlySet<WaitingMissionDTO> ConflictSet { get; }
        public IReadOnlySet<FlyingMissionDTO> FlyingConflictMissions { get; }
        public int NegotiationsCount { get; }
        public Priority CurrentPriority { get; }

        public TimeSpan TotalWaitTime { get; protected set; }

        public TimeSpan DoneFlyTime { get; protected set; }

        public Point2D CurrentPosition { get; protected set; }



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
            CurrentPosition = Path.StartPoint;
            DoneFlyTime = TimeSpan.Zero;
            TotalWaitTime = Age - DoneFlyTime;
            NegotiationsCount = 0;
            DroneTimestamp = DateTime.Now;
        }

        public DroneStateDTO(DateTime droneTimestamp,
                             IActorRef nodeRef,
                             MissionPath path,
                             TimeSpan age,
                             IReadOnlySet<IActorRef> knownNodes,
                             IReadOnlySet<WaitingMissionDTO> conflictSet,
                             IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                             int negotiationsCount,
                             Priority currentPriority,
                             TimeSpan totalWaitTime,
                             TimeSpan doneFlyTime,
                             Point2D currentPosition)
        {
            DroneTimestamp = droneTimestamp;
            NodeRef = nodeRef;
            Path = path;
            Age = age;
            KnownNodes = knownNodes;
            ConflictSet = conflictSet;
            FlyingConflictMissions = flyingConflictMissions;
            NegotiationsCount = negotiationsCount;
            CurrentPriority = currentPriority;
            TotalWaitTime = totalWaitTime;
            DoneFlyTime = doneFlyTime;
            CurrentPosition = currentPosition;
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
    }

    public class InitStateDTO : DroneStateDTO
    {
        public IReadOnlySet<IActorRef> MissingConnections { get; }

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

        public InitStateDTO(DateTime droneTimestamp,
                            IActorRef nodeRef,
                            MissionPath path,
                            TimeSpan age,
                            IReadOnlySet<IActorRef> knownNodes,
                            IReadOnlySet<WaitingMissionDTO> conflictSet,
                            IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                            int negotiationsCount,
                            Priority currentPriority,
                            TimeSpan totalWaitTime,
                            TimeSpan doneFlyTime,
                            Point2D currentPosition,
                            IReadOnlySet<IActorRef> missingConnections) : 
            base(droneTimestamp,
                 nodeRef,
                 path,
                 age,
                 knownNodes,
                 conflictSet,
                 flyingConflictMissions,
                 negotiationsCount,
                 currentPriority,
                 totalWaitTime,
                 doneFlyTime,
                 currentPosition)
        {
            MissingConnections = missingConnections;
        }
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

        public NegotiateStateDTO(DateTime droneTimestamp,
                                 IActorRef nodeRef,
                                 MissionPath path,
                                 TimeSpan age,
                                 IReadOnlySet<IActorRef> knownNodes,
                                 IReadOnlySet<WaitingMissionDTO> conflictSet,
                                 IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                                 int negotiationsCount,
                                 Priority currentPriority,
                                 TimeSpan totalWaitTime,
                                 TimeSpan doneFlyTime,
                                 Point2D currentPosition) : 
            base(droneTimestamp,
                 nodeRef,
                 path,
                 age,
                 knownNodes,
                 conflictSet,
                 flyingConflictMissions,
                 negotiationsCount,
                 currentPriority,
                 totalWaitTime,
                 doneFlyTime,
                 currentPosition)
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

        public WaitingStateDTO(DateTime droneTimestamp,
                                 IActorRef nodeRef,
                                 MissionPath path,
                                 TimeSpan age,
                                 IReadOnlySet<IActorRef> knownNodes,
                                 IReadOnlySet<WaitingMissionDTO> conflictSet,
                                 IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                                 int negotiationsCount,
                                 Priority currentPriority,
                                 TimeSpan totalWaitTime,
                                 TimeSpan doneFlyTime,
                                 Point2D currentPosition) :
            base(droneTimestamp,
                 nodeRef,
                 path,
                 age,
                 knownNodes,
                 conflictSet,
                 flyingConflictMissions,
                 negotiationsCount,
                 currentPriority,
                 totalWaitTime,
                 doneFlyTime,
                 currentPosition)
        {
        }
    }

    public class FlyingStateDTO : DroneStateDTO 
    {
        public TimeSpan ExtimatedRemainingFlyingTime { get; }

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
            DoneFlyTime = doneFlyingTime;
            CurrentPosition = currentPosition;
            ExtimatedRemainingFlyingTime = remainingFlyingTime;
        }

        public FlyingStateDTO(DateTime droneTimestamp,
                              IActorRef nodeRef,
                              MissionPath path,
                              TimeSpan age,
                              IReadOnlySet<IActorRef> knownNodes,
                              IReadOnlySet<WaitingMissionDTO> conflictSet,
                              IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                              int negotiationsCount,
                              Priority currentPriority,
                              TimeSpan totalWaitTime,
                              TimeSpan doneFlyTime,
                              Point2D currentPosition,
                              TimeSpan remainingFlyingTime) : 
            base(droneTimestamp,
                 nodeRef,
                 path,
                 age,
                 knownNodes,
                 conflictSet,
                 flyingConflictMissions,
                 negotiationsCount,
                 currentPriority,
                 totalWaitTime,
                 doneFlyTime,
                 currentPosition)
        {
            ExtimatedRemainingFlyingTime = remainingFlyingTime;
        }
    }

    public class ExitStateDTO : DroneStateDTO
    {
        public bool IsMissionAccomplished { get; }

        public DroneStateDTO PrecedentState { get; }

        public string Motivation { get; }

        public bool Error { get; }

        internal ExitStateDTO(DroneActorContext droneContext,
            int negotiationsCount,
            bool isMissionAccomplished, 
            string motivation, 
            bool error, 
            DroneStateDTO precedentState)
            : base(droneContext,
                  new HashSet<WaitingMission>(), new HashSet<FlyingMission>(),
                  negotiationsCount, Priority.NullPriority)
        {
            IsMissionAccomplished = isMissionAccomplished;
            PrecedentState = precedentState;
            Motivation = motivation;
            Error = error;
            CurrentPosition = precedentState.CurrentPosition;
            DoneFlyTime = precedentState.DoneFlyTime;
        }

        public ExitStateDTO(DateTime droneTimestamp,
                            IActorRef nodeRef,
                            MissionPath path,
                            TimeSpan age,
                            IReadOnlySet<IActorRef> knownNodes,
                            IReadOnlySet<WaitingMissionDTO> conflictSet,
                            IReadOnlySet<FlyingMissionDTO> flyingConflictMissions,
                            int negotiationsCount,
                            Priority currentPriority,
                            TimeSpan totalWaitTime,
                            TimeSpan doneFlyTime,
                            Point2D currentPosition,
                            bool isMissionAccomplished, 
                            DroneStateDTO precedentState, 
                            string motivation, 
                            bool error) : 
            base(droneTimestamp,
                 nodeRef,
                 path,
                 age,
                 knownNodes,
                 conflictSet,
                 flyingConflictMissions,
                 negotiationsCount,
                 currentPriority,
                 totalWaitTime,
                 doneFlyTime,
                 currentPosition)
        {
            IsMissionAccomplished = isMissionAccomplished;
            PrecedentState = precedentState;
            Motivation = motivation;
            Error = error;
        }
    }
}
