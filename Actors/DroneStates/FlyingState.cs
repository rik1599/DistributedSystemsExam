using Actors.Messages.External;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;
using MathNet.Spatial.Euclidean;
using System.Diagnostics;

namespace Actors.DroneStates
{
    internal class FlyingState : DroneActorState
    {
        public FlyingState(DroneActor droneActor, IActorRef droneActorRef,
            ConflictSet conflictSet, FlyingMissionsMonitor flyingMissionsMonitor)
            : base(droneActor, droneActorRef, conflictSet, flyingMissionsMonitor)
        {
        }

        internal override DroneActorState RunState()
        {
            foreach (IActorRef node in ConflictSet.GetNodes())
            {
                node.Tell(new FlyingResponse(MissionPath));
            }

            // TODO: avvia attore per la gestione del nodo in volo

            return this;
        }

        internal override DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            sender.Tell(new FlyingResponse(GetCurrentPath()));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!DroneActor.Nodes.Contains(sender))
                DroneActor.Nodes.Add(sender);

            return this;
        }

        internal override DroneActorState OnReceive(ConnectResponse msg, IActorRef sender)
        {
            // TODO: aggiungi errore
            
            return this;
        }

        internal override DroneActorState OnReceive(FlyingResponse msg, IActorRef sender)
        {
            // TODO: aggiungi errore

            Debug.Assert(GetCurrentPath().ClosestConflictPoint(msg.Path) == null);

            return this;
        }

        internal override DroneActorState OnReceive(MetricMessage msg, IActorRef sender)
        {
            // TODO: aggiungi errore

            sender.Tell(new MetricMessage(Priority.InfinitePriority));
            sender.Tell(new FlyingResponse(GetCurrentPath()));

            return this;
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            // TODO: aggiungi errore

            sender.Tell(new FlyingResponse(GetCurrentPath()));

            return this;
        }

        private MissionPath GetCurrentPath()
        {
            // TODO: effettua ASK ad attore volo
            Point2D currentPosition = GetCurrentPosition();

            return new MissionPath(
                    currentPosition,
                    MissionPath.EndPoint,
                    MissionPath.Speed
                    );
        }

        private Point2D GetCurrentPosition()
        {
            throw new NotImplementedException();
        }
    }
}
