using MathNet.Spatial.Euclidean;
using Akka.Actor;

namespace Environment
{
    public class Mission
    {
        public Point2D Start { get; private set; }
        public Point2D End { get; private set; }

        public Mission(Point2D start, Point2D stop)
        {
            Start = start;
            End = stop;
        }
    }

    public class MissionMessage
    {
        public Mission Mission { get; private set; }
        public Address Host { get; private set; }

        public MissionMessage(Mission mission, Address host)
        {
            Mission = mission;
            Host = host;
        }
    }
}
