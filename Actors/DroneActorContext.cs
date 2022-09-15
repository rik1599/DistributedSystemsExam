using Actors.MissionPathPriority;
using Actors.Utils;
using Akka.Actor;
using Akka.Event;

namespace Actors
{
    sealed class DroneActorContext
    {
        private readonly DateTime _timeSpawn = DateTime.Now;
        internal ITimerScheduler Timers { get; }
        internal IActorContext Context { get; }
        internal DebugLog Log { get; }
        internal ISet<IActorRef> Nodes { get; }
        internal Mission ThisMission { get; }
        internal TimeSpan Age
        {
            get { return DateTime.Now - _timeSpawn; }
        }

        public DroneActorContext(IActorContext context, ISet<IActorRef> nodes, Mission thisMission, ITimerScheduler timers)
        {
            Context = context;
            Log = new(context.GetLogger());
            Nodes = nodes;
            ThisMission = thisMission;
            Timers = timers;
        }
    }
}
