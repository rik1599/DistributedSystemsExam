using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DroneStates;
using Actors.Messages.Register;
using Actors.Messages.User;
using Actors.Messages.StateChangeNotifier;
using Actors.StateChangeNotifier;

namespace Actors
{
    public abstract class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler? Timers { get; set; }

        private DroneActorState? _droneState;

        private NotificationProtocol? _notificationProtocol;

        /// <summary>
        /// Eventuale riferimento a chi mi ha spawnato
        /// </summary>
        private readonly IActorRef? _spawner;

        public DroneActor(IActorRef? spawner = null)
        {
            _spawner = spawner;
        }

        /// <summary>
        /// Algoritmo di schedulazione delle partenze
        /// </summary>
        protected void AlgorithmRunBehaviour(ISet<IActorRef> nodes, MissionPath missionPath)
        {
            var droneContext = new DroneActorContext(Context, nodes, new WaitingMission(Self, missionPath, Priority.NullPriority), Timers!);

            // avvio il servizio di notifica
            _notificationProtocol = new NotificationProtocol(droneContext, 
                _spawner is null ? null : new HashSet<IActorRef>() { _spawner }
                );

            // avvio lo stato iniziale
            _droneState = DroneActorState.CreateInitState(droneContext, Timers!,
                _notificationProtocol.GetStateChangeNotifierVisitor()).RunState();

            ReceiveMainProtocolMessages();
            ReceiveInternalMessage();
            HandleUserRequests();
            HandleNotificationProtocol();
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi esterni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveMainProtocolMessages()
        {
            // la modalità di gestione dei messaggi dipende dallo stato del drone
            Receive<ConnectRequest> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ConnectResponse>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<FlyingResponse> (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<MetricMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<WaitMeMessage>  (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<ExitMessage>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi interni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveInternalMessage()
        {
            Receive<InternalFlyIsSafeMessage>(msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<InternalMissionEnded>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
            Receive<InternalTimeoutEnded>    (msg => _droneState = _droneState!.OnReceive(msg, Sender));
        }

        private void HandleUserRequests()
        {
            Receive<GetStatusRequest>(msg =>
            {
                StateDTOBuilderVisitor visitor = new StateDTOBuilderVisitor();
                _droneState!.PerformVisit(visitor);

                Sender.Tell(new GetStatusResponse(visitor.StateDTO!));
            });

            Receive<CancelMissionRequest>(msg =>
            {
                _droneState = DroneActorState.CreateExitState(
                    _droneState!, false,
                    "Mission cancelled", 
                    false).RunState();

                Sender.Tell(new CancelMissionResponse());
            });
        }

        private void HandleNotificationProtocol()
        {
            Receive<INotificationProtocolMessage>(msg => _notificationProtocol!.OnReceive(msg));
        }

        public static Props Props(IActorRef repository, MissionPath missionPath, IActorRef? spawner=null)
        {
            return Akka.Actor.Props.Create(() => new RegisterDroneActor(repository, missionPath, spawner));
        }

        public static Props Props(ISet<IActorRef> nodes, MissionPath missionPath, IActorRef? spawner = null)
        {
            return Akka.Actor.Props.Create(() => new SimpleDroneActor(nodes, missionPath, spawner));
        }
    }

    /// <summary>
    /// Un attore che per funzionare si appoggia ad un registro di nodi. 
    ///
    /// Quando spawna, comunica il suo ingresso al registro e riceve come output 
    /// una lista di tutti i nodi. 
    /// </summary>
    internal class RegisterDroneActor : DroneActor
    {
        /// <summary>
        /// Procedura iniziale di connessione ad un repository
        /// per reperire tutti i possibili nodi.
        /// </summary>
        public RegisterDroneActor(IActorRef repository, MissionPath missionPath, IActorRef? spawner = null) : base(spawner)
        {
            try
            {
                Task<RegisterResponse> t = repository.Ask<RegisterResponse>(new RegisterRequest(Self), TimeSpan.FromSeconds(10));
                t.Wait();

                AlgorithmRunBehaviour(t.Result.Nodes, missionPath);
            }
            catch (AskTimeoutException)
            {
                Context.GetLogger().Info($"Timeout scaduto. Impossibile comunicare con il repository all'indirizzo {repository}");
                Context.Stop(Self);
            }
        }
    }

    /// <summary>
    /// Attore semplice che riceve in input una lista di nodi. 
    /// </summary>
    internal class SimpleDroneActor : DroneActor
    {
        public SimpleDroneActor(ISet<IActorRef> nodes, MissionPath missionPath, IActorRef? spawner = null) : base(spawner)
        {
            AlgorithmRunBehaviour(nodes.ToHashSet(), missionPath);
        }
    }
}
