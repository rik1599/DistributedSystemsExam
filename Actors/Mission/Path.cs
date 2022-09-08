using MathNet.Spatial.Euclidean;

namespace Actors.Mission
{
    public class Path
    {
        public Point2D StartPoint { get; private set; }
        public Point2D EndPoint { get; private set; }
        public float Speed { get; private set; }

        public Path(Point2D startPoint, Point2D endPoint, float speed)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Speed = speed;
        }

        public Point2D? ClosestConflictPoint(Path p)
        {
            return null;
        }

        public TimeSpan TimeDistance(Point2D p)
        {
            return TimeSpan.Zero;
        }
    }
}
