using Actors.Messages.External;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class NegotiateState : DroneActorState
    {
        public NegotiateState(DroneActor droneActor, IActorRef droneActorRef) : base(droneActor, droneActorRef)
        {
        }

        internal override DroneActorState InitStateProcedure()
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
