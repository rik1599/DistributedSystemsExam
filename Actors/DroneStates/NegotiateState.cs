using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class NegotiateState : DroneActorState
    {
        private ISet<IActorRef> _expectedMetrics;
        private ISet<IActorRef> _expectedIntentions;
        private Priority Priority;

        public NegotiateState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor) 
            : base(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor)
        {
            _expectedMetrics = conflictSet.GetAllNodes();
            _expectedIntentions = new HashSet<IActorRef>();
            Priority = PriorityCalculator.CalculatePriority(DroneActor.ThisMission, DroneActor.Age, );
        }

        internal override DroneActorState RunState()
        {
            
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
