using Actors.Messages.External;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class InitState : DroneActorState
    {

        private ConflictSet _conflictSet;

        private FlyingMissionsMonitor _flyingMissionsMonitor;

        private ISet<IActorRef> _expectedConnectResponses;


        internal InitState(DroneActor droneActor, IActorRef droneActorRef) : base(droneActor, droneActorRef)
        {
            _conflictSet = new ConflictSet();

            _flyingMissionsMonitor = new FlyingMissionsMonitor(droneActor.ThisMission,
                new FlyingSet(), droneActor.Timers);

            _expectedConnectResponses = DroneActor.OtherNodes.ToHashSet();
        }

        internal override DroneActorState RunState()
        {
            // se non ho vicini, posso passare direttamente allo stato successivo
            if (_expectedConnectResponses.Count == 0)
                return CreateNegotiateState(DroneActor, DroneActorRef, 
                    _conflictSet, _flyingMissionsMonitor).RunState();

            var connectRequest = new ConnectRequest(DroneActor.ThisMission.Path);

            foreach (var node in _expectedConnectResponses)
            {
                node.Tell(connectRequest);
            }

            // TODO: far partire timeout

            return this;
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
