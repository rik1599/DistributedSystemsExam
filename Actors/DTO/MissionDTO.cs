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

        public MissionDTO(IActorRef nodeRef, MissionPath missionPath)
        {
            NodeRef = nodeRef;
            MissionPath = missionPath;
        }

        public static MissionDTO GetFlyingMissionDTO(FlyingMission flyingMission, Mission thisMission)
        {
            return new FlyingMissionDTO(flyingMission.NodeRef, flyingMission.Path, flyingMission.GetRemainingTimeForSafeStart(thisMission));
        }

        public static MissionDTO GetWaitingMissionDTO(WaitingMission waitingMission)
        {
            return new WaitingMissionDTO(waitingMission.NodeRef, waitingMission.Path, waitingMission.Priority);
        }
    }

    /// <summary>
    /// DTO della missione in volo con stima del tempo rimanente per la partenza sicura.
    /// </summary>
    public class FlyingMissionDTO : MissionDTO
    {
        public TimeSpan RemainingTimeForSafeStart { get; }

        public FlyingMissionDTO(IActorRef nodeRef, MissionPath missionPath, TimeSpan remainingTimeForSafeStart) 
            : base(nodeRef, missionPath)
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

        public WaitingMissionDTO(IActorRef nodeRef, MissionPath missionPath, Priority priority)
            : base(nodeRef, missionPath)
        {
            Priority = priority;
        }
    }

    
}
