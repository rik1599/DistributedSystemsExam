using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;

namespace UnitTests
{
    public class MissionPathTests
    {
        [Fact]
        public void CheckParallel()
        {
            MissionPath missionPathA = new MissionPath(
                Point2D.Origin,
                new Point2D(2, 2),
                1.0F
                );

            MissionPath missionPathB = new MissionPath(
                new Point2D(1, -1),
                new Point2D(3, 1),
                1.0F
                );

            Assert.Null(missionPathA.ClosestConflictPoint(missionPathB));
        }

        [Fact]
        public void CheckIncident()
        {
            MissionPath missionPathA = new MissionPath(
                Point2D.Origin,
                new Point2D(2, 2),
                1.0F
                );

            MissionPath missionPathB = new MissionPath(
                new Point2D(0, 2),
                new Point2D(2, 0),
                1.0F
                );

            Point2D? pc = missionPathA.ClosestConflictPoint(missionPathB);
            Assert.NotNull(pc);
            Assert.Equal(new Point2D(1, 1), pc);
        }
    }
}