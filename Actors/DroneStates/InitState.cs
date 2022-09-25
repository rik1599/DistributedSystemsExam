using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal class InitState : DroneActorState
    {

        private const string _timeoutKeyName = "connectResponse-timeout";
        private readonly ISet<IActorRef> _expectedConnectResponses;

        internal IReadOnlySet<IActorRef> GetMissingConnectResponses() => _expectedConnectResponses.ToHashSet();

        internal InitState(DroneActorContext context, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor, 
            IDroneStateVisitor changeStateNotifier) 
            : base(context, conflictSet, flyingMissionsMonitor, changeStateNotifier)
        {
            _expectedConnectResponses = ActorContext.Nodes.ToHashSet();
        }

        internal override DroneActorState RunState()
        {
            if (_expectedConnectResponses.Count > 0)
            {
                // invio richiesta di connessione a tutti i nodi noti
                var connectRequest = new ConnectRequest(MissionPath);

                foreach (var node in _expectedConnectResponses)
                {
                    node.Tell(connectRequest, ActorRef);
                }

                ActorContext.StartMessageTimeout(_timeoutKeyName, _expectedConnectResponses.Count);
            }

            // notifico cambio di stato
            PerformVisit(ChangeStateNotifier);

            // se non ho vicini, annullo i timeout e posso passare
            // direttamente allo stato successivo
            return NextState();
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            // verifico se c'è conflitto (ed eventualmente aggiungo al conflict set)
            if (MissionPath.ClosestConflictPoint(msg.Path) != null)
            {
                ConflictSet.AddMission(sender, msg.Path);
            }

            _expectedConnectResponses.Remove(sender);

            return NextState();
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            base.OnReceive(msg, sender);

            _expectedConnectResponses.Remove(sender);

            return NextState();
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            // (sono ancora in stato di inizializzazione,
            // quindi non partecipo alle negoziazioni)
            sender.Tell(new MetricMessage(Priority.NullPriority, LastNegotiationRound));

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

            return NextState();
        }

        private DroneActorState NextState()
        {
            // se non attendo altre risposte, posso passare allo stato successivo
            if (_expectedConnectResponses.Count == 0)
            {
                ActorContext.CancelMessageTimeout(_timeoutKeyName);
                return CreateNegotiateState(this).RunState();
            }

            return this;
        }

        public override void PerformVisit(IDroneStateVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
