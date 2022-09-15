﻿using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.Messages.External;
using Actors.Messages.Register;

namespace UnitTests.ActorsTests.TwoActors
{
    /// <summary>
    /// Rappresentazione di situazioni nelle quali un nuovo nodo 
    /// spawna e deve gestirsi dei semplici conflitti (con un unico
    /// altro nodo).
    /// </summary>
    public class SimpleConflictTests : TestKit
    {
        /// <summary>
        /// cielo libero 1:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo.
        /// </summary>
        [Fact]
        public void FreeSky1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef>();
            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            var msg = ExpectMsg<RegisterRequest>();
            msg.Actor.Tell(new RegisterResponse(nodes), TestActor);

            // quando gli richiedo di connettersi, mi aspetto sia partito in volo
            msg.Actor.Tell(new ConnectRequest(missionB), TestActor);
            ExpectMsgFrom<FlyingResponse>(msg.Actor, TimeSpan.FromDays(1));


            // mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(msg.Actor, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


        /// <summary>
        /// cielo libero 2:
        /// 
        /// un drone spawna, conosce un nodo ma non va in conflitto, va in volo.
        /// </summary>
        [Fact]
        public void FreeSky2()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(0, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(25, 25), 10.0f);

            var nodes = new HashSet<IActorRef> { TestActor };
            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta che non prevede conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto un'uscita per missione completata (in un tempo ragionevole)
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }

        /// <summary>
        /// cielo libero 3:
        /// 
        /// un drone spawna, conosce un nodo in volo ma non problematico, va in volo a sua volta.
        /// </summary>
        [Fact]
        public void FreeSky3()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(0, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(25, 25), 10.0f);

            var nodes = new HashSet<IActorRef> { TestActor };
            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta in volo che non prevede conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new FlyingResponse(missionB), TestActor);

            // mi aspetto un'uscita per missione completata (in un tempo ragionevole)
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }


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

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto che mi richieda di negoziare (rispondo con priorità <)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue - 5, TestActor), 0));

            // la negoziazione la vince lui => volo
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

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
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

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // comunico mio volo (lungo, che comporta attesa)
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

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            ExpectMsgFrom<ConnectRequest>(subject);
            // rispondo al drone che sono in volo!
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa (più lunga), mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 10));

            Sys.Terminate();
        }

        /// <summary>
        /// Conflitto semplice perso 4:
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, 
        /// lo perde, riceve prima un wait me e poi (dopo un po') la notifica 
        /// sul volo.
        /// </summary>
        [Fact]
        public void SimpleConflictLoose4()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var nodes = new HashSet<IActorRef>{ TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // invece che comunicare il mio volo, comunico un wait me
            subject.Tell(new WaitMeMessage());

            // aspetto un po' e poi volo (per un volo non troppo lungo)
            Thread.Sleep(1500);
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa, mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }

        /// <summary>
        /// conflitto semplice dove due nodi hanno lo stesso ID
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, 
        /// in negoziazione emerge che hanno lo stesso ID (dovrebbe vincere
        /// chi ce l'ha più piccolo)
        /// </summary>
        [Fact]
        public void SimpleConflictSameID1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var nodes = new HashSet<IActorRef> { TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(TestActor, missionA), "droneProva");
            ExpectMsg<RegisterRequest>();
            subject.Tell(new RegisterResponse(nodes), TestActor);

            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            // gli rispondo con la stessa metrica ma ID diverso
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue, TestActor), 0));

            if (TestActor.CompareTo(subject) < 0)
            {
                // se ho l'ID minore, mi aspetto di vincere
                Thread.Sleep(500);
                subject.Tell(new FlyingResponse(missionB));

                // Dopo una ragionevole attesa (più lunga), mi aspetto un'uscita per missione completata
                ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 10));

            } else
            {
                // altrimenti mi aspetto che vinca lui

                // Dopo una ragionevole attesa (più lunga), mi aspetto un'uscita per missione completata
                ExpectMsgFrom<FlyingResponse>(subject);
                ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));
            }

            Sys.Terminate();
        }
    }
}
