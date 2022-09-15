using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DroneStates;
using Actors.Messages.Register;
using Actors.Utils;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }

        private DroneActorState _droneState;
        private readonly DateTime _timeSpawn = DateTime.Now;

        internal IActorContext DroneContext { get; private set; }
        internal DebugLog Log { get; } = new(Context.GetLogger());
        internal ISet<IActorRef> Nodes { get; private set; }
        internal Mission ThisMission { get; private set; }
        internal TimeSpan Age
        {
            get { return DateTime.Now - _timeSpawn; }
        }

        public DroneActor(IActorRef repository, MissionPath missionPath)
        {
            Nodes = new HashSet<IActorRef>();
            ThisMission = new WaitingMission(Self, missionPath, Priority.NullPriority);
            DroneContext = Context;

            RegisterBehaviour(repository);

            // avvio lo stato iniziale
            _droneState = DroneActorState.CreateInitState(this).RunState();
        }

        private void RegisterBehaviour(IActorRef repository)
        {
            try
            {
                Nodes = 
                    repository.Ask<RegisterResponse>(new RegisterRequest(Self), TimeSpan.FromSeconds(10))
                    .Result.Nodes;

                ReceiveExternalMessages();
                ReceiveInternalMessage();
            }
            catch (AskTimeoutException)
            {
                Log.Info($"Timeout scaduto. Impossibile comunicare con il repository all'indirizzo {repository}");
                Context.Stop(Self);
            }
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi esterni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveExternalMessages()
        {
            // la modalità di gestione dei messaggi dipende dallo stato del drone
            Receive<ConnectRequest>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<ConnectResponse>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<FlyingResponse>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<MetricMessage>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<WaitMeMessage>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<ExitMessage>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<MissionFinishedMessage>(msg => {});
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi interni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveInternalMessage()
        {
            Receive<InternalFlyIsSafeMessage>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<InternalMissionEnded>    (msg => _droneState = _droneState.OnReceive(msg, Sender));
        }

        public static Props Props(IActorRef environment, MissionPath missionPath)
        {
            return Akka.Actor.Props.Create(() => new DroneActor(environment, missionPath));
        }
    }
}
