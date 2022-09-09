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
        internal ISet<IActorRef> Others { get; private set; }
        internal Mission Mission { get; private set; }

        private DroneActorState _droneState;

        public DroneActor(ISet<IActorRef> others, MissionPath missionPath)
        {
            Others = others;
            Mission = new WaitingMission(Self, missionPath, Priority.NullPriority);
            _droneState = new InitState(this);

            Receive<ConnectRequest> (msg => _droneState = _droneState.OnReceive(msg, TODO, TODO, TODO, TODO, TODO, TODO));
            Receive<ConnectResponse>(msg => _droneState = _droneState.OnReceive(msg, TODO, TODO, TODO, TODO, TODO));
            Receive<FlyingResponse> (msg => _droneState = _droneState.OnReceive(msg, TODO, TODO, TODO, TODO));
            Receive<MetricMessage>  (msg => _droneState = _droneState.OnReceive(msg, TODO, TODO, TODO));
            Receive<WaitMeMessage>  (msg => _droneState = _droneState.OnReceive(msg, TODO, TODO));
            Receive<ExitMessage>    (msg => _droneState = _droneState.OnReceive(msg, TODO));
        }
    }
}
