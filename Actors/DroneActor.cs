using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;
using Actors.DroneStates;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        private readonly ILoggingAdapter _log = Context.GetLogger();

        internal DateTime TimeSpawn { get; private set; } = DateTime.Now;

        internal ISet<IActorRef> Nodes { get; private set; }
        internal Mission ThisMission { get; private set; }

        private DroneActorState _droneState;

        public DroneActor(ISet<IActorRef> others, MissionPath missionPath)
        {
            Nodes = others;
            ThisMission = new WaitingMission(Self, missionPath, Priority.NullPriority);

            // avvio lo stato iniziale
            _droneState = InitState.CreateInitState(this, Self).RunState();                 

            // la modalità di gestione dei messaggi dipende dallo stato del drone
            Receive<ConnectRequest> (msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<ConnectResponse>(msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<FlyingResponse> (msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<MetricMessage>  (msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<WaitMeMessage>  (msg => _droneState = _droneState.OnReceive(msg, Self));
            Receive<ExitMessage>    (msg => _droneState = _droneState.OnReceive(msg, Self));
        }
    }
}
