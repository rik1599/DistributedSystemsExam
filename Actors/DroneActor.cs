using Akka.Actor;

namespace Actors
{
    public class DroneActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }
    }
}
