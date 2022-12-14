using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    public abstract class DroneActorState
    {
        internal DroneActorContext ActorContext { get; }
        internal int LastNegotiationRound { get; set; }

        /// <summary>
        /// tool per la gestione delle tratte in conflitto (con cui posso negoziare)
        /// </summary>
        internal ConflictSet ConflictSet { get; }

        /// <summary>
        /// Tool per la gestione delle tratte in volo (che devo attendere)
        /// </summary>
        internal FlyingMissionsMonitor FlyingMissionsMonitor { get; }

        /// <summary>
        /// Strumento da utilizzare per notificare un cambio di stato
        /// </summary>
        protected IDroneStateVisitor ChangeStateNotifier { get; }

        /// <summary>
        /// shortcut per la tratta della missione corrente
        /// </summary>
        protected MissionPath MissionPath { get => ActorContext.ThisMission.Path; }

        /// <summary>
        /// shortcut per il riferimento al nodo corrente
        /// </summary>
        protected IActorRef ActorRef { get => ActorContext.Context.Self; }

        internal DroneActorState(DroneActorContext context, ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor, IDroneStateVisitor changeStateNotifier)
        {
            ActorContext = context;
            ConflictSet = conflictSet;
            FlyingMissionsMonitor = flyingMissionsMonitor;
            LastNegotiationRound = 0;
            ChangeStateNotifier = changeStateNotifier;
        }

        protected DroneActorState(DroneActorState state)
        {
            ActorContext = state.ActorContext;
            ConflictSet = state.ConflictSet;
            FlyingMissionsMonitor = state.FlyingMissionsMonitor;
            LastNegotiationRound = state.LastNegotiationRound;
            ChangeStateNotifier = state.ChangeStateNotifier;
        }

        #region Factory methods

        internal static DroneActorState CreateInitState(DroneActorContext context, ITimerScheduler timer, IDroneStateVisitor changeStateNotifier)
            => new InitState(
                context, 
                new ConflictSet(), 
                new FlyingMissionsMonitor(context.ThisMission, new FlyingSet(), timer),
                changeStateNotifier
                );

        internal static DroneActorState CreateNegotiateState(DroneActorState precedentState)
            => new NegotiateState(precedentState);

        internal static DroneActorState CreateWaitingState(DroneActorState precedentState, Priority priority)
            => new WaitingState(precedentState, priority);

        internal static DroneActorState CreateFlyingState(DroneActorState precedentState)
            => new FlyingState(precedentState);

        internal static DroneActorState CreateExitState(DroneActorState precedentState, bool isMissionAccomplished, string motivation, bool error=false) 
            => new ExitState(precedentState, isMissionAccomplished, motivation, error);

        #endregion

        #region External Messages

        internal virtual DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            // rispondo con la mia tratta
            sender.Tell(new ConnectResponse(MissionPath));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!ActorContext.Nodes.Contains(sender))
                ActorContext.Nodes.Add(sender);

            // verifico se c'è conflitto (ed eventualmente aggiungo al conflict set)
            if (MissionPath.ClosestConflictPoint(msg.Path) != null)
            {
                ConflictSet.AddMission(sender, msg.Path);
            }

            return this;
        }

        internal virtual DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            // è in volo => non ci devo più negoziare
            WaitingMission? eventualMission = ConflictSet.RemoveMission(sender);

            // se ho conflitto, aggiungo il nodo alla lista di nodi in volo
            if (MissionPath.ClosestConflictPoint(msg.Path) is not null)
            {
                FlyingMissionsMonitor.MakeMissionFly(
                    eventualMission ??
                    new WaitingMission(sender, msg.Path, Priority.InfinitePriority)
                    );
            }

            return this;
        }

        internal virtual DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            ConflictSet.RemoveMission(sender);
            FlyingMissionsMonitor.CancelMission(sender);
            ActorContext.Nodes.Remove(sender);

            return this;
        }

        internal abstract DroneActorState OnReceive(ConnectResponse msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(MetricMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender);

        #endregion

        #region Internal Messages

        internal virtual DroneActorState OnReceive(InternalFlyIsSafeMessage msg, IActorRef sender)
        {
            FlyingMissionsMonitor.OnReceive(msg);
            return this;
        }

        internal virtual DroneActorState OnReceive(InternalMissionEnded msg, IActorRef sender)
        {
            return this;
        }

        internal virtual DroneActorState OnReceive(InternalTimeoutEnded msg, IActorRef sender)
        {

            return CreateExitState(this, false, $"ERRORE: timeout {msg.TimerKey} scaduto!", true).RunState();
        }

        #endregion

        internal abstract DroneActorState RunState();

        public abstract void PerformVisit(IDroneStateVisitor visitor);
    }
}
