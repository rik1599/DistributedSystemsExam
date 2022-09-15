using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;
using MathNet.Spatial.Euclidean;
using System.Diagnostics;

namespace Actors.DroneStates
{
    internal class FlyingState : DroneActorState
    {
        /// <summary>
        /// Riferimento all'attore che sta gestendo il "volo fisico".
        /// 
        /// Viene avviato assieme alla procedura di volo e mi serve
        /// per richiedere la posizione corrente.
        /// </summary>
        private IActorRef? _flyingDroneActor;

        private bool _isMissionEnd = false;
        
        public FlyingState(DroneActorState precedentState): base(precedentState) {}

        internal override DroneActorState RunState()
        {
            DroneActor.Log.Info("Sto partendo");
            foreach (var node in ConflictSet.GetNodes())
            {
                node.Tell(new FlyingResponse(MissionPath));
            }

            // Avvio un attore figlio che gestisce il processo del volo
            _flyingDroneActor = DroneActor.DroneContext.ActorOf(
                FlyingDroneActor.Props(DroneActor.ThisMission, DroneActorRef), "fly-actor");

            // lo supervisiono (in modo da rilevare quando e come termina)
            DroneActor.DroneContext.WatchWith(_flyingDroneActor, new InternalMissionEnded());

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

            sender.Tell(new MetricMessage(Priority.InfinitePriority, LastNegotiationRound));
            sender.Tell(new FlyingResponse(GetCurrentPath()));

            return this;
        }

        internal override DroneActorState OnReceive(WaitMeMessage msg, IActorRef sender)
        {
            // TODO: aggiungi errore

            sender.Tell(new FlyingResponse(GetCurrentPath()));

            return this;
        }

        internal override DroneActorState OnReceive(InternalMissionEnded msg, IActorRef sender)
        {
            if (_isMissionEnd)
            {
                // messaggio duplicato
                return this;
            }

            _isMissionEnd = true;

            // comunico la mia uscita a tutti i nodi noti 
            foreach (IActorRef node in DroneActor.Nodes)
            {
                node.Tell(new MissionFinishedMessage());
            }

            // TODO: termina 

            return this;
        }


        private MissionPath GetCurrentPath()
        {
            return new MissionPath(
                GetCurrentPosition(),
                MissionPath.EndPoint,
                MissionPath.Speed
                );
        }

        /// <summary>
        /// Richiedi la posizione all'attore che gestisce il volo 
        /// (la richiesta viene effettuata tramite ASK, pertanto
        /// potrebbe non ritornare immediatamente una risposta)
        /// </summary>
        /// <returns></returns>
        private Point2D GetCurrentPosition()
        {
            return _flyingDroneActor
                .Ask<InternalPositionResponse>(new InternalPositionRequest())
                .Result.Position;
        }
    }
}
