using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace Actors.MissionPathPriority
{
    struct PointDistance : IComparable<PointDistance>
    {
        public Point2D PointOnMe { get; private set; }
        public Point2D PointOnOther { get; private set; }

        public double Distance
        {
            get { return PointOnMe.DistanceTo(PointOnOther); }
        }

        public PointDistance(Point2D pointOnMe, Point2D pointOnOther)
        {
            PointOnMe = pointOnMe;
            PointOnOther = pointOnOther;
        }

        public int CompareTo(PointDistance other)
        {
            return (int)(Distance - other.Distance);
        }
    }

    /// <summary>
    /// Tratta che un drone vuole percorrere, si tratta
    /// di uno spostamento da un punto A ad un punto B ad una certa
    /// velocità. 
    /// 
    /// Permette di calcolare agevolmente conflitti (con un margine) e 
    /// di ottenere la distanza (in termini temporali) da un certo punto.
    /// </summary>
    public class MissionPath
    {
        public Point2D StartPoint { get; private set; }
        public Point2D EndPoint { get; private set; }

        private LineSegment2D PathSegment { 
            get { return new LineSegment2D(StartPoint, EndPoint); }
        }
        
        /// <summary>
        /// Velocità alla quale il drone si sposta. Si misura in 
        /// unità spaziali al secondo.
        /// </summary>
        public float Speed { get; private set; }

        public const float MarginDistance = 5.0f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="speed">Velocità in unità spaziali al secondo</param>
        public MissionPath(Point2D startPoint, Point2D endPoint, float speed)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Speed = speed;
        }

        /// <summary>
        /// Calcola se esiste un punto di conflitto con un altra tratta 
        /// e - in caso - determina qual è il punto di conflitto più 
        /// vicino al mio punto di partenza. 
        /// </summary>
        /// <param name="p"></param>
        /// <returns>
        /// L'eventuale punto di conflitto più vicino al mio punto di partenza.
        /// 
        /// Se il punto coincide con il mio punto di partenza o quello 
        /// di arrivo si ritorna direttamente il punto di partenza o di arrivo.
        /// 
        /// Se non ci sono conflitti tra le due tratte, si ritorna null.
        /// </returns>
        public Point2D? ClosestConflictPoint(MissionPath p, double margin = MarginDistance)
        {
            var minDistanceSegment = new SegmentDistanceCalculator(PathSegment, p.PathSegment).ComputeMinDistance();

            if (minDistanceSegment.Length < margin && PathSegment.TryIntersect(minDistanceSegment, out var conflictPoint, Angle.FromRadians(0)))
                return conflictPoint;
            else
                return null;
        }

        /// <summary>
        /// Calcola quanto tempo ci metto a raggiungere
        /// un certo punto a partire dal mio punto 
        /// di partenza.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public TimeSpan TimeDistance(Point2D p)
        {
            return TimeSpan.Zero;
        }
    }
}
