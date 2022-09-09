using Actors.Messages.External;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    public abstract class DroneActorState
    {
        protected DroneActor DroneActor { get; set; }
        protected IActorRef DroneActorRef { get; set; }

        protected DroneActorState(DroneActor droneActor, IActorRef droneActorRef)
        {
            DroneActor = droneActor;
            DroneActorRef = droneActorRef;
        }

        #region Factory methods

        public static DroneActorState CreateInitState(DroneActor droneActor, IActorRef droneActorRef)
            => new InitState(droneActor, droneActorRef);

        public static DroneActorState CreateNegotiateState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
            => new NegotiateState(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor);

        public static DroneActorState CreateWaitingState(DroneActor droneActor, IActorRef droneActorRef)
            => new WaitingState(droneActor, droneActorRef);

        public static DroneActorState CreateFlyingState(DroneActor droneActor, IActorRef droneActorRef)
            => new FlyingState(droneActor, droneActorRef);

        #endregion


        internal abstract DroneActorState OnReceive(ConnectRequest msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(ConnectResponse msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(FlyingResponse msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(MetricMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(ExitMessage msg, IActorRef sender);

        internal abstract DroneActorState RunState();
    }
}
