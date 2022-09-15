

using Actors;
using Actors.Messages.External;
using Actors.Messages.Register;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Environment;
using MathNet.Spatial.Euclidean;

namespace UnitTests.ActorTests.Register
{
    public class RegisterTest : TestKit
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

            var register = Sys.ActorOf(DronesRepositoryActor.Props());

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

            // quando gli richiedo di connettersi, mi aspetto sia partito in volo
            subject.Tell(new ConnectRequest(missionB), TestActor);
            ExpectMsgFrom<FlyingResponse>(subject);


            // mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(0, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(25, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
        /// un drone spawna, conosce un nodo in volo e in conflitto ma 
        /// probabilmente safe in tempi ragionevoli, va in volo a sua volta.
        /// </summary>
        [Fact]
        public void FreeSky3()
        {
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(0, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(25, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(new Point2D(0, 25), new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 30), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(new Point2D(0, 25), new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 30), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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
        /// conflitto semplice dove due nodi hanno la stessa metrica
        /// 
        /// un drone spawna, conosce un nodo, osserva il conflitto, 
        /// in negoziazione emerge che hanno lo stesso ID (dovrebbe vincere
        /// chi ce l'ha più piccolo)
        /// </summary>
        [Fact]
        public void SimpleConflictSameMetric1()
        {
            // mi registro
            var register = Sys.ActorOf(DronesRepositoryActor.Props());
            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var subject = Sys.ActorOf(RegisterDroneActor.Props(register, missionA), "droneProva");

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

            }
            else
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
