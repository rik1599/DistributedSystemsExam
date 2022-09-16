using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace Actors.MissionPathPriority
{
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
        public Point2D StartPoint { get; }
        public Point2D EndPoint { get; }
        
        /// <summary>
        /// Velocità alla quale il drone si sposta. Si misura in 
        /// unità spaziali al secondo.
        /// </summary>
        public float Speed { get; }

        internal const float MarginDistance = 5.0f;


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
            if (PathSegment().TryIntersect(p.PathSegment(), out var conflictPoint, Angle.FromRadians(0)))
                return conflictPoint;

            var minDistanceSegment = new SegmentDistanceCalculator(PathSegment(), p.PathSegment()).ComputeMinDistance();

            if (minDistanceSegment.Length < margin && 
                PathSegment().TryIntersect(minDistanceSegment, out var minDistanceConflictPoint, Angle.FromRadians(0)))
                return minDistanceConflictPoint;
            else
                return null;
        }

        public LineSegment2D PathSegment()
        {
            return new LineSegment2D(StartPoint, EndPoint);
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
            // (la velocità si esprime in unità al secondo)
            return TimeSpan.FromSeconds(
                new Line2D(StartPoint, p).Length / Speed
                );
        }
    }
}
