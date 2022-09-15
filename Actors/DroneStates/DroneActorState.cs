using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    internal abstract class DroneActorState
    {
        protected DroneActor Actor { get; private set; }
        protected int LastNegotiationRound { get; set; }

        /// <summary>
        /// tool per la gestione delle tratte in conflitto (con cui posso negoziare)
        /// </summary>
        protected ConflictSet ConflictSet { get; private set; }

        /// <summary>
        /// Tool per la gestione delle tratte in volo (che devo attendere)
        /// </summary>
        protected FlyingMissionsMonitor FlyingMissionsMonitor { get; private set; }

        /// <summary>
        /// shortcut per la tratta della missione corrente
        /// </summary>
        protected MissionPath MissionPath { get => Actor.ThisMission.Path; }

        /// <summary>
        /// shortcut per il riferimento al nodo corrente
        /// </summary>
        protected IActorRef ActorRef { get => Actor.DroneContext.Self; }

        protected DroneActorState(DroneActor droneActor, ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
        {
            Actor = droneActor;
            ConflictSet = conflictSet;
            FlyingMissionsMonitor = flyingMissionsMonitor;
            LastNegotiationRound = 0;
        }

        protected DroneActorState(DroneActorState precedentState)
        {
            Actor = precedentState.Actor;
            ConflictSet = precedentState.ConflictSet;
            FlyingMissionsMonitor = precedentState.FlyingMissionsMonitor;
            LastNegotiationRound = precedentState.LastNegotiationRound;
        }

        #region Factory methods

        public static DroneActorState CreateInitState(DroneActor droneActor)
            => new InitState(
                droneActor, 
                new ConflictSet(), 
                new FlyingMissionsMonitor(droneActor.ThisMission, new FlyingSet(), droneActor.Timers)
                );

        public static DroneActorState CreateNegotiateState(DroneActorState precedentState)
            => new NegotiateState(precedentState);

        public static DroneActorState CreateWaitingState(DroneActorState precedentState, Priority priority)
            => new WaitingState(precedentState, priority);

        public static DroneActorState CreateFlyingState(DroneActorState precedentState)
            => new FlyingState(precedentState);

        #endregion

        #region External Messages

        internal virtual DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            // rispondo con la mia tratta
            sender.Tell(new ConnectResponse(MissionPath));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!Actor.Nodes.Contains(sender))
                Actor.Nodes.Add(sender);

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
            Actor.Nodes.Remove(sender);

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

        #endregion

        internal abstract DroneActorState RunState();
    }
}
