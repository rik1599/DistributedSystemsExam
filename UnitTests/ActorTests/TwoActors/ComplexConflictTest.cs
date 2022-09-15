using Actors;
using Actors.Messages.External;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.ActorTests.TwoActors
{
    /// <summary>
    /// Rappresentazione di situazioni più elaborate di vario genere, 
    /// sempre basate su due attori.
    /// </summary>
    public class ComplexConflictTest : TestKit
    {
        /// <summary>
        /// Gestione del secondo round di negoziazione 1
        /// 
        /// un drone spawna, prima di riuscire a contattare un nodo riceve da lui una
        /// metrica, perde una negoziazione, riceve la connessione e poi vince la 
        /// seconda negoziazione.
        /// </summary>
        [Fact]
        public void SecondNegotiationRound1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            // invece di inviare una risposta, invio una metrica
            subject.Tell(new MetricMessage(new Priority(500, TestActor), 0));

            // mi aspetto come messaggio una metrica (di valore nullo e di round 0)
            MetricMessage metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            Assert.Equal(double.MinValue, metricMsg.Priority.MetricValue);
            Assert.Equal(0, metricMsg.RelativeRound); 

            // termino la negoziazione con un wait me
            subject.Tell(new WaitMeMessage());


            // ora invio la connect response
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // ad un certo punto mi aspetto un'altra metrica (di un altro round che voglio perdere)
            MetricMessage metricMsg2 = ExpectMsgFrom<MetricMessage>(subject);
            Assert.Equal(1, metricMsg2.RelativeRound);
            subject.Tell(new MetricMessage(new Priority(metricMsg2.Priority.MetricValue - 5, TestActor), 0));

            // visto che ho perso, attendo una flying response
            ExpectMsgFrom<FlyingResponse>(subject);

            // (volo)

            // mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


        /// <summary>
        /// Conflitto senza punto di intersezione 1:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto 
        /// (anche se le tratte non si incrociano propriamente), 
        /// lo perde e aspetta per volare (un tempo irrisorio).
        /// </summary>
        [Fact]
        public void NotIntersectConflict1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(15, 0), new Point2D(15, 14), 10.0f);

            var nodes = new HashSet<IActorRef>
            {
                TestActor
            };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // mi aspetto una connessione (a cui rispondo con una tratta con
            // conflitto, anche se senza intersezione)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunico mio volo (non troppo lungo)
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa, mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


    }
}
