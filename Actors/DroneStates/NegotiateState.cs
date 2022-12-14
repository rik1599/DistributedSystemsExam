using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal struct MetricSenderPair
    {
        public IActorRef Sender { get; }
        public MetricMessage MetricMessage { get; }

        public MetricSenderPair(IActorRef sender, MetricMessage metricMessage)
        {
            Sender = sender;
            MetricMessage = metricMessage;
        }
    }

    internal class NegotiateState : DroneActorState
    {
        private readonly ISet<IActorRef> _expectedMetrics;
        private readonly ISet<IActorRef> _expectedIntentions;
        private readonly IDictionary<IActorRef, int> _negotiationRounds;
        private readonly IList<MetricSenderPair> _metricMessages;

        private const string _metricTimeoutKey = "metricMessage-timeout";
        private const string _intentionsTimeoutKey = "intentionMessage-timeout";

        /// <summary>
        /// Priorità utilizzata in questo singolo round di negoziazione
        /// </summary>
        private readonly Priority _priority;

        internal Priority GetPriority() => _priority;

        public NegotiateState(DroneActorState precedentState) 
            : base(precedentState)
        {
            _expectedMetrics = ConflictSet.GetNodes();
            _expectedIntentions = new HashSet<IActorRef>();
            _priority = PriorityCalculator.CalculatePriority(
                ActorContext.ThisMission, 
                ActorContext.Age, 
                ConflictSet.GetMissions(), 
                FlyingMissionsMonitor.GetFlyingMissions()
            );
            _negotiationRounds = new Dictionary<IActorRef, int>();
            _metricMessages = new List<MetricSenderPair>();
        }

        internal override DroneActorState RunState()
        {
            LastNegotiationRound++;
            if (_expectedMetrics.Count > 0)
            {
                foreach (var node in _expectedMetrics)
                {
                    node.Tell(new MetricMessage(_priority, LastNegotiationRound));
                }

                ActorContext.StartMessageTimeout(_metricTimeoutKey, _expectedMetrics.Count);
            }

            // notifico cambio di stato
            PerformVisit(ChangeStateNotifier);

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
            var s = NextState();
            return s;
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            if (!_negotiationRounds.ContainsKey(sender))
            {
                _negotiationRounds.Add(sender, msg.RelativeRound);
            }

            if (msg.RelativeRound < _negotiationRounds[sender])
            {
                return this;
            }

            if (msg.RelativeRound > _negotiationRounds[sender])
            {
                _metricMessages.Add(new MetricSenderPair(sender, msg));
                return this;
            }

            _ = _expectedMetrics.Remove(sender);
            var mission = ConflictSet.GetMission(sender);
            mission!.Priority = msg.Priority;

            if (_priority.CompareTo(mission!.Priority) < 0)
            {
                _expectedIntentions.Add(sender);
            }

            return NextState();
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            _ = _expectedMetrics.Remove(sender);
            _ = _expectedIntentions.Remove(sender);
            return NextState();
        }

        internal override DroneActorState OnReceive(InternalFlyIsSafeMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        internal override DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            _ = base.OnReceive(msg, sender);
            return NextState();
        }

        private DroneActorState NextState()
        {
            // attendo tutte le metriche
            if (_expectedMetrics.Count > 0)
                return this;
            else
            {
                ActorContext.CancelMessageTimeout(_metricTimeoutKey);
                ActorContext.StartMessageTimeout(_intentionsTimeoutKey, _expectedIntentions.Count);
            }

            // se ho ricevuto tutte le metriche, ho vinto tutte le negoziazioni
            // e non attendo droni in volo, posso partire
            if (FlyingMissionsMonitor.GetFlyingMissions().Count == 0 && ConflictSet.GetGreaterPriorityMissions(_priority).Count == 0)
            {
                ActorContext.CancelMessageTimeout(_intentionsTimeoutKey);
                ResendMetricsToMailBox();
                return CreateFlyingState(this).RunState();
            }

            // se ho ricevuto tutte le metriche [, non ho vinto tutte le negoziazioni]
            // e ho ricevuto le intenzioni da tutti coloro che hanno metrica > me, 
            // entro in stato di attesa 
            if (_expectedIntentions.Count == 0)
            {
                ActorContext.CancelMessageTimeout(_intentionsTimeoutKey);
                ResendMetricsToMailBox();
                return CreateWaitingState(this, _priority).RunState();
            }
            else 
                return this;
        }

        private void ResendMetricsToMailBox()
        {
            foreach (var msg in _metricMessages)
            {
                ActorRef.Tell(msg.MetricMessage, msg.Sender);
            }
        }

        public override void PerformVisit(IDroneStateVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
