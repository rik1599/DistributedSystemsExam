using Actors.MissionPathPriority;
using Akka.Actor;


namespace Actors.DTO
{
    /// <summary>
    /// Rappresentazione della missione come DTO. 
    /// </summary>
    public class MissionDTO
    {
        public IActorRef NodeRef { get; }
        
        public MissionPath MissionPath { get; }

        protected MissionDTO(Mission mission)
        {
            NodeRef = mission.NodeRef;
            MissionPath = mission.Path;
        }

        public static MissionDTO GetFlyingMissionDTO(FlyingMission flyingMission, Mission thisMission)
        {
            return new FlyingMissionDTO(flyingMission, flyingMission.GetRemainingTimeForSafeStart(thisMission));
        }

        public static MissionDTO GetWaitingMissionDTO(WaitingMission waitingMission)
        {
            return new WaitingMissionDTO(waitingMission, waitingMission.Priority);
        }
    }

    /// <summary>
    /// DTO della missione in volo con stima del tempo rimanente per la partenza sicura.
    /// </summary>
    public class FlyingMissionDTO : MissionDTO
    {
        public TimeSpan RemainingTimeForSafeStart { get; }

        public FlyingMissionDTO(Mission mission, TimeSpan remainingTimeForSafeStart) : base(mission)
        {
            RemainingTimeForSafeStart = remainingTimeForSafeStart;
        }
    }

    /// <summary>
    /// DTO della missione in attesa. E' caratterizzando anche da una priorità
    /// </summary>
    public class WaitingMissionDTO : MissionDTO
    {
        public Priority Priority { get; }

        public WaitingMissionDTO(Mission mission, Priority priority) : base(mission)
        {
            Priority = priority;
        }
    }

    
}
