using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.Messages.Internal;
using Actors.DroneStates;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }

        private DroneActorState _droneState;
        private readonly DateTime _timeSpawn = DateTime.Now;

        internal IActorContext DroneContext { get; private set; }
        internal ILoggingAdapter Log { get; } = Context.GetLogger();
        internal ISet<IActorRef> Nodes { get; private set; }
        internal Mission ThisMission { get; private set; }
        internal TimeSpan Age
        {
            get { return DateTime.Now - _timeSpawn; }
        }

        public DroneActor(ISet<IActorRef> others, MissionPath missionPath)
        {
            Nodes = others;
            ThisMission = new WaitingMission(Self, missionPath, Priority.NullPriority);
            DroneContext = Context;

            // avvio lo stato iniziale
            _droneState = DroneActorState.CreateInitState(this, Self).RunState();

            ReceiveExternalMessages();
            ReceiveInternalMessage();
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi esterni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveExternalMessages()
        {
            // la modalità di gestione dei messaggi dipende dallo stato del drone
            Receive<ConnectRequest> (msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<ConnectResponse>(msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<FlyingResponse> (msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<MetricMessage>  (msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<WaitMeMessage>  (msg => _droneState = _droneState.OnReceive(msg, Sender));
            Receive<ExitMessage>    (msg => _droneState = _droneState.OnReceive(msg, Sender));
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

        public static Props Props(ISet<IActorRef> others, MissionPath missionPath)
        {
            return Akka.Actor.Props.Create(() => new DroneActor(others, missionPath));
        }
    }
}
