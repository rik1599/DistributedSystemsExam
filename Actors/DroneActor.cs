using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.External;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly DateTime _timeSpawn = DateTime.Now;
        private readonly ISet<IActorRef> _others;
        private readonly Mission _mission;

        private readonly ISet<IActorRef> _waitingNodes;

        public DroneActor(ISet<IActorRef> others, MissionPath missionPath)
        {
            _others = others;
            _mission = new WaitingMission(Self, missionPath, Priority.NullPriority);

            Receive<ConnectRequest>(msg => { });
            Receive<ConnectResponse>(msg => { });
            Receive<FlyingResponse>(msg => { });
            Receive<MetricMessage>(msg => { });
            Receive<WaitMeMessage>(msg => { });
            Receive<ExitMessage>(msg => { });
        }
    }
}
