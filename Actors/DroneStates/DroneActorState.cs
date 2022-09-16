using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal abstract class DroneActorState
    {
        protected DroneActorContext ActorContext { get; }
        protected int LastNegotiationRound { get; set; }

        /// <summary>
        /// tool per la gestione delle tratte in conflitto (con cui posso negoziare)
        /// </summary>
        protected ConflictSet ConflictSet { get; }

        /// <summary>
        /// Tool per la gestione delle tratte in volo (che devo attendere)
        /// </summary>
        protected FlyingMissionsMonitor FlyingMissionsMonitor { get; }

        /// <summary>
        /// shortcut per la tratta della missione corrente
        /// </summary>
        protected MissionPath MissionPath { get => ActorContext.ThisMission.Path; }

        /// <summary>
        /// shortcut per il riferimento al nodo corrente
        /// </summary>
        protected IActorRef ActorRef { get => ActorContext.Context.Self; }

        protected DroneActorState(DroneActorContext context, ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
        {
            ActorContext = context;
            ConflictSet = conflictSet;
            FlyingMissionsMonitor = flyingMissionsMonitor;
            LastNegotiationRound = 0;
        }

        protected DroneActorState(DroneActorState state)
        {
            ActorContext = state.ActorContext;
            ConflictSet = state.ConflictSet;
            FlyingMissionsMonitor = state.FlyingMissionsMonitor;
            LastNegotiationRound = state.LastNegotiationRound;
        }

        #region Factory methods

        public static DroneActorState CreateInitState(DroneActorContext context, ITimerScheduler timer)
            => new InitState(
                context, 
                new ConflictSet(), 
                new FlyingMissionsMonitor(context.ThisMission, new FlyingSet(), timer)
                );

        public static DroneActorState CreateNegotiateState(DroneActorState precedentState)
            => new NegotiateState(precedentState);

        public static DroneActorState CreateWaitingState(DroneActorState precedentState, Priority priority)
            => new WaitingState(precedentState, priority);

        public static DroneActorState CreateFlyingState(DroneActorState precedentState)
            => new FlyingState(precedentState);

        public static DroneActorState CreateExitState(DroneActorState precedentState, bool isMissionAccomplished, String motivation) 
            => new ExitState(precedentState, isMissionAccomplished, motivation);

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

            return CreateExitState(this, false, $"ERRORE: timeout {msg.TimerKey} scaduto!").RunState();
        }

        #endregion

        internal abstract DroneActorState RunState();
    }
}
