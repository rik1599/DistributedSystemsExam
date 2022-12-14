using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.DTO;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI;
using DroneSystemAPI.APIClasses;
using Actors.Messages.Register;
using Actors.Messages.External;
using DroneSystemAPI;

namespace UnitTests.API.Deprecated
{
    /*
    
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

            var config = SystemConfigs.DroneConfig;
            
            // creo il registro
            var register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository");

            // creo il tool per lo spawn di missioni
            var spawner = new MissionSpawner(Sys, 
                register, SimpleMissionAPI.Factory(), config);

            // spawno due missioni 
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA")!;
            IMissionAPI b = spawner.SpawnHere(missionB, "DroneB")!;

            // richiedo ad entrambe lo stato
            _ = a.GetCurrentStatus().Result;
            _ = b.GetCurrentStatus().Result;

            Sys.Terminate();
        }

        /// <summary>
        /// Crea un'API per connetterti ad una missione già esistente.
        /// </summary>
        [Fact]
        public void ConnectToExistingMission()
        {         
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            var droneConfig = SystemConfigs.DroneConfig;
            droneConfig.SystemName = "test";

            // creo il registro
            var register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository");

            // spawno una missione manualmente
            var spawner = Sys.ActorOf<SpawnerActor>("spawner");
            spawner.Ask(new SpawnActorRequest(DroneActor.Props(register, missionA), "DroneA")).Wait();

            IMissionAPI? a = new MissionProvider(Sys, droneConfig).TryConnectToExistent(Host.GetTestHost(), "DroneA");
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);

            Sys.Terminate();
        }

        /// <summary>
        /// Spawna una missione in remoto (sempre nell'ambiente test, 
        /// ma con la funzione apposita).
        /// </summary>
        [Fact]
        public void SpawnMissionRemote()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            var config = SystemConfigs.DroneConfig;
            config.SystemName = "test";

            new DeployPointInitializer(Sys).Init();

            // creo il registro
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository");
            ;
            // uso il tool per spawnare in remoto una missione
            // e ricavare un'istanza dell'API
            MissionSpawner spawner = new MissionSpawner(Sys,
                register, SimpleMissionAPI.Factory(), config);

            IMissionAPI? a = spawner.SpawnRemote(Host.GetTestHost(), missionA, "DroneA");
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);

            Sys.Terminate();
        }

        /// <summary>
        /// Spawno una missione, la faccio volare e la annullo.
        /// </summary>
        [Fact]
        public void CancelMission()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            var config = SystemConfigs.DroneConfig;
            config.SystemName = "test";

            // creo il registro (e mi ci iscrivo)
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository");


            register.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(register);

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new MissionSpawner(Sys,
                register, SimpleMissionAPI.Factory(), config);

            // spawno una missione
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA")!;

            // mi aspetto la connessione e la negoziazione
            ExpectMsgFrom<ConnectRequest>(a.GetDroneRef());
            a.GetDroneRef().Tell(new ConnectResponse(missionB));
            ExpectMsgFrom<MetricMessage>(a.GetDroneRef());
            a.GetDroneRef().Tell(new MetricMessage(Priority.NullPriority, 1));

            // mentre vola, cancello la missione
            // (mi aspetto una sua uscita)
            ExpectMsgFrom<FlyingResponse>(a.GetDroneRef());
            a.Cancel().Wait();
            ExpectMsgFrom<ExitMessage>(a.GetDroneRef());

            ExpectNoMsg(new TimeSpan(0, 0, 7));

            Sys.Terminate();
        }
    } */
}
