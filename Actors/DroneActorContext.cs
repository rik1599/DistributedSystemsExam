using Actors.MissionPathPriority;
using Actors.Utils;
using Akka.Actor;
using Akka.Event;
using Actors.Messages.Internal;

namespace Actors
{
    internal sealed class DroneActorContext
    {
        private readonly DateTime _timeSpawn = DateTime.Now;
        private readonly ITimerScheduler _timers;

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
            _timers = timers;
        }

        internal void StartMessageTimeout(string key, int count)
        {
            _timers.StartSingleTimer(
                key, 
                new InternalTimeoutEnded(key),
                count * TimeSpan.FromSeconds(10));
        }

        internal void CancelMessageTimeout(string key)
        {
            _timers.Cancel(key);
        }
    }
}
