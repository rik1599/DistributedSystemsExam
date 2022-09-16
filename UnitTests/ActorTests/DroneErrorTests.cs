using Actors;
using Actors.Messages.External;
using Actors.Messages.Register;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;

namespace UnitTests.ActorTests
{
    /// <summary>
    /// Test di simulazione di timeout dei droni nelle varie fasi
    /// Il TestActor fa da repository e da "vicino" del nodo 
    /// (colui che crasha e provoca il timeout nel drone di prova)
    /// 
    /// Ci si aspetta di ricevere sul TestActor un ExitMessage
    /// </summary>
    public class DroneErrorTests : TestKit
    {
        [Fact]
        public void TimeoutDroneOnConnect()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            _ = Sys.ActorOf(DroneActor.Props(TestActor, missionA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);
            ExpectMsg<ConnectRequest>();

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }

        [Fact]
        public void TimeoutDroneOnNegotiate()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            _ = Sys.ActorOf(DroneActor.Props(TestActor, missionA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);
            
            ExpectMsg<ConnectRequest>();
            LastSender.Tell(new ConnectResponse(missionB), TestActor);

            ExpectMsg<MetricMessage>();

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }

        [Fact]
        public void TimeoutDroneOnIntentions()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            _ = Sys.ActorOf(DroneActor.Props(TestActor, missionA));
            ExpectMsg<RegisterRequest>();

            var nodes = new HashSet<IActorRef>() { TestActor };
            LastSender.Tell(new RegisterResponse(nodes.ToHashSet()), TestActor);

            ExpectMsg<ConnectRequest>();
            LastSender.Tell(new ConnectResponse(missionB), TestActor);

            var msg = ExpectMsg<MetricMessage>();
            LastSender.Tell(new MetricMessage(new Priority(msg.Priority.MetricValue + 1, TestActor), 0), TestActor);

            var startTime = DateTime.Now;
            ExpectMsg<ExitMessage>(TimeSpan.FromSeconds(15));
            Assert.True((DateTime.Now - startTime) >= TimeSpan.FromSeconds(10));
            Sys.Terminate();
        }
    }
}
