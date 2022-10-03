using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;

namespace UnitTests.ActorTests
{
    public class MissionPathTests
    {
        [Fact]
        public void CheckParallel()
        {
            MissionPath missionPathA = new(
                Point2D.Origin,
                new Point2D(3, 1),
                1.0F
                );

            MissionPath missionPathB = new(
                new Point2D(2, 0),
                new Point2D(5, 1),
                1.0F
                );

            Assert.Null(missionPathA.ClosestConflictPoint(missionPathB, 0.003f));
        }

        [Fact]
        public void CheckIncident()
        {
            MissionPath missionPathA = new(
                Point2D.Origin,
                new Point2D(2, 2),
                1.0F
                );

            MissionPath missionPathB = new(
                new Point2D(0, 2),
                new Point2D(2, 0),
                1.0F
                );

            Point2D? pc = missionPathA.ClosestConflictPoint(missionPathB, 0.0001f);
            Assert.NotNull(pc);
            Assert.True(pc is not null && pc.Value.Equals(new Point2D(1, 1), 0.001));
        }

        [Fact]
        public void TestVector()
        {
            var vector = new Point2D(2, 2) - new Point2D(1, 1);
            Console.WriteLine(vector);
        }
    }
}