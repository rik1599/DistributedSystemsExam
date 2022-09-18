using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.DTO;
using DroneSystemAPI.APIClasses.Register;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI;
using DroneSystemAPI.APIClasses.Utils;

namespace UnitTests.API
{
    /// <summary>
    /// Rappresentazione di situazioni nelle quali un nuovo nodo 
    /// spawna e deve gestirsi dei semplici conflitti (con un unico
    /// altro nodo).
    /// </summary>
    public class MissionAPITest : TestKit
    {

        /// <summary>
        /// Semplice tentativo di avvio di una missione 
        /// (e richiesta stato) tramite le API
        /// </summary>
        [Fact]
        public void MissionStart()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RegisterAPI register = new RegisterProvider(Sys).SpawnHere();

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new (Sys, 
                register, SimpleMissionAPI.Factory(), config);

            // spawno due missioni 
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA");
            IMissionAPI b = spawner.SpawnHere(missionB, "DroneB");

            // richiedo ad entrambe lo stato
            _ = a.GetCurrentStatus().Result;
            _ = b.GetCurrentStatus().Result;

            Sys.Terminate();
        }

        [Fact]
        public void ConnectToExistingMission()
        {
            // TODO: aggiusta!
            
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RegisterAPI register = new RegisterProvider(Sys).SpawnHere();

            // spawno una missione manualmente
            var realRef = Sys.ActorOf(
                DroneActor.Props(register.ActorRef, missionA)
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(
                        Address.Parse(Host.GetTestHost().GetSystemAddress(config.DroneSystemName))
                        ))),
                "DroneA");


            // uso il tool per ricavare un'istanza dell'API e le richiedo lo stato
            MissionSpawner spawner = new (Sys,
                register, SimpleMissionAPI.Factory(), config);

            IMissionAPI? a = spawner.TryConnectToExistent(Host.GetTestHost(), "DroneA");
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);

            Sys.Terminate();
        }
        
        /*

        /// <summary>
        /// cielo libero 1:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo 
        /// (e gli richiedo lo stato corrente)
        /// </summary>
        [Fact]
        public void FreeSky1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // gli richiedo lo stato
            Thread.Sleep(250);
            subject.Tell(new GetStatusRequest());
            var msg = ExpectMsgFrom<GetStatusResponse>(subject);

            // mi aspetto che sia in volo
            Assert.Equal(typeof(FlyingStateDTO), msg.StateDTO.GetType());

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

            var nodes = new HashSet<IActorRef> { TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");
            
            // -----------------------------------------------------------------
            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);

            // prima di rispondergli, gli richiedo lo stato
            subject.Tell(new GetStatusRequest());
            var stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(InitStateDTO), stateMsg.StateDTO.GetType());

            subject.Tell(new ConnectResponse(missionB), TestActor);

            // -----------------------------------------------------------------
            // mi aspetto che mi richieda di negoziare (rispondo con priorità <)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            // prima di rispondergli, gli richiedo lo stato
            subject.Tell(new GetStatusRequest());
            stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(NegotiateStateDTO), stateMsg.StateDTO.GetType());

            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue - 5, TestActor), 0));

            // ------------------------------------------------------------------
            // la negoziazione la vince lui => volo
            ExpectMsgFrom<FlyingResponse>(subject);

            // gli richiedo lo stato e mi aspetto sia in volo
            subject.Tell(new GetStatusRequest());
            stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(FlyingStateDTO), stateMsg.StateDTO.GetType());

            // mi aspetto un'uscita per missione completata
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
            var missionA = new MissionPath(new Point2D(0, 30), new Point2D(25, 30), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 40), 10.0f);

            var nodes = new HashSet<IActorRef> { TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // -----------------------------------------------------------------
            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);

            // prima di rispondergli, gli richiedo lo stato
            subject.Tell(new GetStatusRequest());
            var stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(InitStateDTO), stateMsg.StateDTO.GetType());

            subject.Tell(new ConnectResponse(missionB), TestActor);

            // -----------------------------------------------------------------
            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);

            // prima di rispondergli, gli richiedo lo stato
            subject.Tell(new GetStatusRequest());
            stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(NegotiateStateDTO), stateMsg.StateDTO.GetType());

            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // -----------------------------------------------------------------
            // comunico mio volo (lungo, che comporta attesa)
            subject.Tell(new FlyingResponse(missionB));

            // ora mi aspetto che lui sia in waiting
            subject.Tell(new GetStatusRequest());
            stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(WaitingStateDTO), stateMsg.StateDTO.GetType());

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
            var missionA = new MissionPath(new Point2D(0, 30), new Point2D(25, 30), 10.0f);
            var missionB = new MissionPath(new Point2D(5, 0), new Point2D(5, 30), 10.0f);

            var nodes = new HashSet<IActorRef> { TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            ExpectMsgFrom<ConnectRequest>(subject);

            // rispondo al drone che sono in volo!
            subject.Tell(new FlyingResponse(missionB));

            // ora mi aspetto che lui sia in waiting
            subject.Tell(new GetStatusRequest());
            var stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(WaitingStateDTO), stateMsg.StateDTO.GetType());

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

            var nodes = new HashSet<IActorRef> { TestActor };

            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // -------------------------------------------------------------------
            // mi aspetto una connessione (a cui rispondo con una tratta con conflitto)
            ExpectMsgFrom<ConnectRequest>(subject);
            subject.Tell(new ConnectResponse(missionB), TestActor);

            // -------------------------------------------------------------------
            // mi aspetto che mi richieda di negoziare (rispondo con priorità >)
            var metricMsg = ExpectMsgFrom<MetricMessage>(subject);
            subject.Tell(new MetricMessage(new Priority(metricMsg.Priority.MetricValue + 5, TestActor), 0));

            // visto che so che ha perso, mi aspetto sia in 
            // attesa di conoscere le mie intenzioni 
            // (=> ancora in negoriate)
            subject.Tell(new GetStatusRequest());
            var stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(NegotiateStateDTO), stateMsg.StateDTO.GetType());

            // -------------------------------------------------------------------
            // invece che comunicare il mio volo, comunico un wait me
            subject.Tell(new WaitMeMessage());

            // ora mi aspetto che lui sia in waiting
            subject.Tell(new GetStatusRequest());
            stateMsg = ExpectMsgFrom<GetStatusResponse>(subject);
            Assert.Equal(typeof(WaitingStateDTO), stateMsg.StateDTO.GetType());

            // aspetto un po' e poi volo (per un volo non troppo lungo)
            Thread.Sleep(1500);
            subject.Tell(new FlyingResponse(missionB));

            // Dopo una ragionevole attesa, mi aspetto un'uscita per missione completata
            ExpectMsgFrom<MissionFinishedMessage>(subject, new TimeSpan(0, 0, 5));

            Sys.Terminate();
        }*/
    }
}
