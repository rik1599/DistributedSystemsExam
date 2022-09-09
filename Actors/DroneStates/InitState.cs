using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class InitState : DroneActorState
    {

        private ISet<IActorRef> _expectedConnectResponses;


        internal InitState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor) 
            : base(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor)
        {
            _expectedConnectResponses = DroneActor.Nodes.ToHashSet();
        }

        internal override DroneActorState RunState()
        {
            // invio richiesta di connessione a tutti i nodi noti
            var connectRequest = new ConnectRequest(MissionPath);

            foreach (var node in _expectedConnectResponses)
            {
                node.Tell(connectRequest);
            }

            // TODO: far partire timeout

            // se non ho vicini, annullo i timeout e posso passare
            // direttamente allo stato successivo
            return checkIfIReceivedAllResponses(); ;
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            // verifico se c'è conflitto (ed eventualmente aggiungo al conflict set)
            if (MissionPath.ClosestConflictPoint(msg.Path) != null)
            {
                ConflictSet.AddMission(sender, msg.Path);
            }

            _expectedConnectResponses.Remove(sender);

            return checkIfIReceivedAllResponses();
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            base.OnReceive(msg, sender);

            _expectedConnectResponses.Remove(sender);

            return checkIfIReceivedAllResponses();
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            // (sono ancora in stato di inizializzazione,
            // quindi non partecipo alle negoziazioni)
            sender.Tell(new MetricMessage(Priority.NullPriority));

            return this;
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            // (ignoro)
            return this;
        }

        internal override DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _expectedConnectResponses.Remove(sender);

            return checkIfIReceivedAllResponses();
        }

        private DroneActorState checkIfIReceivedAllResponses()
        {
            // se non attendo altre risposte, posso passare allo stato successivo
            if (_expectedConnectResponses.Count == 0)
            {
                // TODO: cancella il timeout su attesa messaggi

                return CreateNegotiateState(DroneActor, DroneActorRef,
                    ConflictSet, FlyingMissionsMonitor).RunState();
            }

            return this;
        } 
    }
}
