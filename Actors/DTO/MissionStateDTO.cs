using Actors.MissionPathPriority;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Actors.DTO
{
    public abstract class MissionStateDTO
    {
        public MissionPath Path { get; internal set; }
        public TimeSpan Age { get; internal set; }
        public IReadOnlySet<IActorRef> KnownNodes { get; internal set; }
        public IReadOnlySet<WaitingMissionDTO> ConflictSet { get; internal set; } = new HashSet<WaitingMissionDTO>();
        public IReadOnlySet<FlyingMissionDTO> FlyingConflictMissions { get; internal set; } = new HashSet<FlyingMissionDTO>();
        public int NegotiationsCount { get; internal set; }
        public Priority LastCalculatedPriority { get; internal set; }

        protected MissionStateDTO(MissionPath path, TimeSpan age, ISet<IActorRef> knownNodes)
        {

        }
    }

    public class InitMissionStateDTO : MissionStateDTO
    {

    }
}
