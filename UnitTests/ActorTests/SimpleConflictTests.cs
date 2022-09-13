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
        /// Conflitto semplice vinto 1:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, lo vince, vola e termina.
        /// </summary>
        [Fact]
        public void SimpleConflictWin1()
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
        /// Conflitto semplice vinto 2:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo.
        /// </summary>
        [Fact]
        public void SimpleConflictWin2()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // quando gli richiedo di connettersi, mi aspetto sia partito in volo
            subject.Tell(new ConnectRequest(missionB), TestActor);
            ExpectMsgFrom<FlyingResponse>(subject);


            // mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


        /// <summary>
        /// Conflitto semplice perso 1:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, 
        /// lo perde e aspetta per volare (un tempo irrisorio).
        /// </summary>
        [Fact]
        public void SimpleConflictLoose1()
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
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa, mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }

        /// <summary>
        /// Conflitto semplice perso 2:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, 
        /// lo perde e aspetta per volare (un tempo un po' più lungo).
        /// </summary>
        [Fact]
        public void SimpleConflictLoose2()
        {
            var missionA = new MissionPath(new Point2D(0, 25), new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 30), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            subject.Tell(new ConnectResponse(missionB), TestActor);
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa (più lunga), mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }


        /// <summary>
        /// Conflitto semplice perso 3:
        /// 
        /// un drone spawna, conosce un nodo e scopre che è in volo, parte al termine
        /// </summary>
        [Fact]
        public void SimpleConflictLoose3()
        {
            var missionA = new MissionPath(new Point2D(0, 25), new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 30), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            // rispondo al drone che sono in volo!
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa (più lunga), mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }
    }
}
