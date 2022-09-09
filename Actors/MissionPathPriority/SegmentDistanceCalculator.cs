using MathNet.Spatial.Euclidean;

namespace Actors.MissionPathPriority
{
    internal class SegmentDistanceCalculator
    {
        private readonly LineSegment2D _segmentA;
        private readonly LineSegment2D _segmentB;

        public SegmentDistanceCalculator(LineSegment2D segmentA, LineSegment2D segmentB)
        {
            _segmentA = segmentA;
            _segmentB = segmentB;
        }

        public LineSegment2D ComputeMinDistance()
        {
            LineSegment2D[] distancesSegments = new LineSegment2D[4];

            distancesSegments[0] = _segmentA.LineTo(_segmentB.StartPoint);
            distancesSegments[1] = _segmentB.LineTo(_segmentA.StartPoint);

            distancesSegments[2] = _segmentA.LineTo(_segmentB.EndPoint);
            distancesSegments[3] = _segmentB.LineTo(_segmentA.EndPoint);

            return distancesSegments.MinBy(s => s.Length);
        }
    }
}
