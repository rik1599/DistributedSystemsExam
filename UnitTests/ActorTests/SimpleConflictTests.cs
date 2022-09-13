using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.Messages.External;

namespace UnitTests.ActorsTests
{
    public class SimpleConflictTests : TestKit
    {         
        /// <summary>
        /// Conflitto semplice 1:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, lo vince, vola e termina.
        /// </summary>
        [Fact]
        public void SimpleConflict1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            subject.Tell(new ConnectResponse(missionB), TestActor);
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue - 5, TestActor), 0));
            ExpectMsgFrom<FlyingResponse>(subject);

            // (volo)

            // mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


        /// <summary>
        /// Conflitto semplice 2:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, lo perde e aspetta per volare.
        /// </summary>
        [Fact]
        public void SimpleConflict2()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            subject.Tell(new ConnectResponse(missionB), TestActor);
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));
            // subject.Tell(new WaitMeMessage());
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa, mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }
       
    }
}
