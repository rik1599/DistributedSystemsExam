using Actors.Messages.External;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class FlyingState : DroneActorState
    {
        public FlyingState(DroneActor droneActor, IActorRef droneActorRef,
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
            : base(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor)
        {
        }

        internal override DroneActorState RunState()
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }

        internal override DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            throw new NotImplementedException();
        }
    }
}
