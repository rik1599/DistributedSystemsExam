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
        internal IActorContext DroneContext { get; private set; }

        private readonly ILoggingAdapter _log = Context.GetLogger();

        internal DateTime TimeSpawn { get; private set; } = DateTime.Now;
        internal TimeSpan Age
        {
            get { return DateTime.Now - TimeSpawn; }
        }

        internal ISet<IActorRef> Nodes { get; private set; }
        internal Mission ThisMission { get; private set; }

        private DroneActorState _droneState;

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
            Receive<ConnectRequest>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<ConnectResponse>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<FlyingResponse>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<MetricMessage>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<WaitMeMessage>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<ExitMessage>(msg => _droneState = _droneState.OnReceive(msg, Self));
        }

        /// <summary>
        /// Ricevi e gestisci tutti i messaggi interni del protocollo 
        /// di schedulazione dei voli.
        /// </summary>
        private void ReceiveInternalMessage()
        {
            Receive<InternalFlyIsSafeMessage>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<InternalMissionEnded>(msg => _droneState = _droneState.OnReceive(msg, Self));
        }
    }
}
