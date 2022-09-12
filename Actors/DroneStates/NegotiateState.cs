using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class NegotiateState : DroneActorState
    {
        private readonly ISet<IActorRef> _expectedMetrics;
        private readonly ISet<IActorRef> _expectedIntentions;
        private readonly Priority _priority;

        public NegotiateState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor) 
            : base(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor)
        {
            _expectedMetrics = conflictSet.GetNodes();
            _expectedIntentions = new HashSet<IActorRef>();
            _priority = PriorityCalculator.CalculatePriority(
                DroneActor.ThisMission, 
                DroneActor.Age, 
                conflictSet.GetMissions(), 
                flyingMissionsMonitor.GetFlyingMissions()
            );
        }

        internal override DroneActorState RunState()
        {
            // invio richiesta di connessione a tutti i nodi noti
            foreach (var node in _expectedMetrics)
            {
                node.Tell(new MetricMessage(_priority));
            }

            // TODO: far partire timeout

            // se non ho vicini, annullo i timeout e posso passare
            // direttamente allo stato successivo
            return NextState();
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            // TODO: errore
            return this;
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            _ = _expectedMetrics.Remove(sender);
            _ = _expectedIntentions.Remove(sender);
            return NextState();
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            _ = _expectedMetrics.Remove(sender);
            var mission = ConflictSet.GetMission(sender);
            mission!.Priority = msg.Priority;
            return NextState();
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            _ = _expectedIntentions.Remove(sender);
            return NextState();
        }

        private DroneActorState NextState()
        {
            if (_expectedMetrics.Count > 0)
                return this;

            if (FlyingMissionsMonitor.GetFlyingMissions().Count == 0 && ConflictSet.GetGreaterPriorityMissions(_priority).Count == 0)
                return CreateFlyingState(DroneActor, DroneActorRef, ConflictSet, FlyingMissionsMonitor);

            if (_expectedIntentions.Count == 0)
                return CreateWaitingState(DroneActor, DroneActorRef, ConflictSet, FlyingMissionsMonitor).RunState();
            else 
                return this;
        }
    }
}
