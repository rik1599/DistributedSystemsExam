using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.Messages.External;

namespace UnitTests
{
    public class ActorTest : TestKit
    {
        [Fact]
        public void MakeDroneFlyTest()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 5.0f);
            var missionB = new MissionPath(new Point2D(100, 0), new Point2D(0, 100), 5.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");
            ExpectMsgFrom<ConnectRequest>(subject);

            subject.Tell(new ConnectResponse(missionB), TestActor);
            ExpectMsgFrom<MetricMessage>(subject);

            subject.Tell(new MetricMessage(Priority.NullPriority, 0));
            ExpectMsgFrom<FlyingResponse>(subject);

            Sys.Terminate();
        }
    }
}
