using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors.DroneStates
{
    public abstract class DroneActorState
    {
        protected DroneActor DroneActor { get; set; }
        protected IActorRef DroneActorRef { get; set; }

        /// <summary>
        /// tool per la gestione delle tratte in conflitto (con cui posso negoziare)
        /// </summary>
        protected ConflictSet ConflictSet { get; set; }

        /// <summary>
        /// Tool per la gestione delle tratte in volo (che devo attendere)
        /// </summary>
        protected FlyingMissionsMonitor FlyingMissionsMonitor { get; set; }


        // shortcut per la tratta Mission Path
        protected MissionPath MissionPath
        {
            get { return DroneActor.ThisMission.Path; }
        }

        protected DroneActorState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
        {
            DroneActor = droneActor;
            DroneActorRef = droneActorRef;
            ConflictSet = conflictSet;
            FlyingMissionsMonitor = flyingMissionsMonitor;
        }

        #region Factory methods

        public static DroneActorState CreateInitState(DroneActor droneActor, IActorRef droneActorRef)
            => new InitState(
                droneActor, droneActorRef, 
                new ConflictSet(), 
                new FlyingMissionsMonitor(droneActor.ThisMission, new FlyingSet(), droneActor.Timers)
                );

        public static DroneActorState CreateNegotiateState(DroneActor droneActor, IActorRef droneActorRef, 
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
            => new NegotiateState(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor);

        public static DroneActorState CreateWaitingState(DroneActor droneActor, IActorRef droneActorRef)
            => new WaitingState(droneActor, droneActorRef);

        public static DroneActorState CreateFlyingState(DroneActor droneActor, IActorRef droneActorRef)
            => new FlyingState(droneActor, droneActorRef);

        #endregion


        internal virtual DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            // rispondo con la mia tratta
            sender.Tell(new ConnectResponse(MissionPath));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!DroneActor.Nodes.Contains(sender))
                DroneActor.Nodes.Add(sender);

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
            if (MissionPath.ClosestConflictPoint(msg.Path) != null)
            {
                FlyingMissionsMonitor.MakeMissionFly(
                    eventualMission ??
                    new WaitingMission(sender, MissionPath, Priority.InfinitePriority)
                    );
            }
           
            return this;
        }

        internal virtual DroneActorState OnReceive(ExitMessage msg, IActorRef sender)
        {
            ConflictSet.RemoveMission(sender);
            FlyingMissionsMonitor.CancelMission(sender);
            DroneActor.Nodes.Remove(sender);

            return this;
        }

        internal abstract DroneActorState OnReceive(ConnectResponse msg, IActorRef sender);

       
        internal abstract DroneActorState OnReceive(MetricMessage msg, IActorRef sender);

        internal abstract DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender);

        internal abstract DroneActorState RunState();
    }
}
