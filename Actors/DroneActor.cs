using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Event;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly DateTime _timeSpawn = DateTime.Now;
        private readonly ISet<IActorRef> _others;
        private readonly Mission _mission;

        public DroneActor(ISet<IActorRef> others, MissionPath missionPath)
        {
            _others = others;
            _mission = new WaitingMission(Self, missionPath, Priority.NullPriority);
        }

        #region Behaviours

        private void InitBehaviour()
        {

        }

        private void NegotiateBehaviour()
        {

        }

        private void WaitingBehaviour()
        {

        }

        private void FlyingBehaviour()
        {

        }
        #endregion
    }
}
