using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class InitState : DroneActorState
    {

        private ConflictSet _conflictSet;

        private FlyingMissionsMonitor _flyingMissionsMonitor;

        private ISet<IActorRef> _expectedConnectResponses;

        // shortcut per la tratta Mission Path
        private MissionPath MissionPath { 
            get { return DroneActor.ThisMission.Path; } 
        }


        internal InitState(DroneActor droneActor, IActorRef droneActorRef) : base(droneActor, droneActorRef)
        {
            _conflictSet = new ConflictSet();

            _flyingMissionsMonitor = new FlyingMissionsMonitor(droneActor.ThisMission,
                new FlyingSet(), droneActor.Timers);

            _expectedConnectResponses = DroneActor.Nodes.ToHashSet();
        }

        internal override DroneActorState RunState()
        {
            // se non ho vicini, posso passare direttamente allo stato successivo
            if (_expectedConnectResponses.Count == 0)
                return CreateNegotiateState(DroneActor, DroneActorRef, 
                    _conflictSet, _flyingMissionsMonitor).RunState();

            // invio richiesta di connessione a tutti i nodi noti
            var connectRequest = new ConnectRequest(MissionPath);

            foreach (var node in _expectedConnectResponses)
            {
                node.Tell(connectRequest);
            }

            // TODO: far partire timeout

            return this;
        }

        internal override DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            // rispondo con la mia tratta
            sender.Tell(new ConnectResponse(MissionPath));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!DroneActor.Nodes.Contains(sender))  
                DroneActor.Nodes.Add(sender);

            // verifico se c'è conflitto (ed eventualmente aggiungo al conflict set)
            var conflictPoint = MissionPath.ClosestConflictPoint(msg.Path);
            if (conflictPoint != null)
            {
                _conflictSet.AddMission(sender, msg.Path);
            }

            return this;
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
