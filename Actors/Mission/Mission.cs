using Akka.Actor;
using MathNet.Spatial.Euclidean;

namespace Actors.Mission
{
    public abstract class Mission
    {
        protected IActorRef NodeRef { get; private set; }

        protected Path Path { get; private set; }

        protected Mission(IActorRef nodeRef, Path path)
        {
            NodeRef = nodeRef;
            Path = path;
        }
    }

    public class WaitingMission : Mission
    {
        public Priority Priority { get; set; }

        public WaitingMission(IActorRef nodeRef, Path path, Priority priority) : base(nodeRef, path)
        {
            Priority = priority;
        }
    }

    public class FlyingMission : Mission
    {
        private readonly DateTime _startTime;

        public FlyingMission(IActorRef nodeRef, Path path, DateTime startTime) : base(nodeRef, path)
        {
            _startTime = startTime;
        }

        public TimeSpan GetRemainingTime()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetRemainingTimeToPoint(Point2D p)
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetRemainingTimeForSafeStart(Mission m)
        {
            throw new NotImplementedException();
        }
    }
}
