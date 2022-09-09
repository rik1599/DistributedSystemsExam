using Actors.Messages.External;
using Akka.Actor;

namespace Actors.DroneStates
{
    public abstract class DroneActorState
    {
        protected DroneActor DroneActor { get; set; }
        protected IActorRef DroneActorRef { get; set; }

        internal DroneActorState(DroneActor droneActor, IActorRef droneActorRef)
        {
            DroneActor = droneActor;
            DroneActorRef = droneActorRef;
        }

        internal abstract DroneActorState OnReceive(ConnectRequest msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(ConnectResponse msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(FlyingResponse msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(MetricMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(ExitMessage msg, IActorRef sender);

        internal abstract DroneActorState InitStateProcedure();
    }
}
