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

        /// <summary>
        /// Ultimo valore della posizione del drone registrato. Si tiene
        /// così in caso che la missione termini (e l'attore per la gestione
        /// del volo non sia più disponibile) si possa ritornare
        /// qualcosa quando viene chiamato il metodo GetCurrentPosition.
        /// </summary>
        private Point2D _lastPositionCache;

        private readonly DateTime _startTime = DateTime.Now;
        internal TimeSpan DoneFlyTime() => _startTime - DateTime.Now;
        internal TimeSpan RemainingFlyTime() => MissionPath.ExpectedDuration() - DoneFlyTime();
        
        public FlyingState(DroneActorState precedentState): base(precedentState) 
        {
            _lastPositionCache = MissionPath.StartPoint;
        }

        internal override DroneActorState RunState()
        {
            ActorContext.Log.Warning("Sto partendo");
            foreach (var node in ConflictSet.GetNodes())
            {
                node.Tell(new FlyingResponse(MissionPath));
            }

            // Avvio un attore figlio che gestisce il processo del volo
            _flyingDroneActor = ActorContext.Context.ActorOf(
                FlyingDroneActor.Props(ActorContext.ThisMission, ActorRef), "fly-actor");

            // lo supervisiono (in modo da rilevare quando e come termina)
            // TODO: gestisci errore
            ActorContext.Context.Watch(_flyingDroneActor);

            // notifico cambio di stato
            PerformVisit(ChangeStateNotifier);

            return this;
        }

        internal override DroneActorState OnReceive(ConnectRequest msg, IActorRef sender)
        {
            sender.Tell(new FlyingResponse(GetCurrentPath()));

            // se non conosco già il nodo, lo aggiungo alla lista
            if (!ActorContext.Nodes.Contains(sender))
                ActorContext.Nodes.Add(sender);

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
            // se il messaggio duplicato, non faccio nulla
            if (_isMissionEnd)
                return this;

            _isMissionEnd = true;

            _lastPositionCache = msg.Position;

            // termino l'attore che gestisce il volo e cancello il riferimento
            _flyingDroneActor.Tell(PoisonPill.Instance);
            _flyingDroneActor = null;   
            
            return CreateExitState(this, true, "Mission ENDED! Killing myself").RunState();
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
        internal Point2D GetCurrentPosition()
        {
            // se l'attore del volo non esiste, uso il valore in cache
            if (_flyingDroneActor is null)
                return _lastPositionCache;

            Task<InternalPositionResponse> t = _flyingDroneActor.Ask<InternalPositionResponse>(
                new InternalPositionRequest(), new TimeSpan(0, 0, 10));

            t.Wait();

            if (t.IsCompleted)
            {
                _lastPositionCache = t.Result.Position;
            }

            // (se non sono riuscito a contattare l'attore, 
            // uso il valore in cache)
            return _lastPositionCache;
        }

        public override void PerformVisit(IDroneStateVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
