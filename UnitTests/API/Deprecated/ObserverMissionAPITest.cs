using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.DTO;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;
using DroneSystemAPI;

namespace UnitTests.API.Deprecated
{
    /*
    /// <summary>
    /// Rappresentazione di situazioni nelle quali un nuovo nodo 
    /// spawna e deve gestirsi dei semplici conflitti (con un unico
    /// altro nodo).
    /// </summary>
    public class ObserverMissionAPITest : TestKit
    {

        #region verifica observer
        /// <summary>
        /// Avvio di una missione (in locale) e monitoraggio 
        /// dei vari passaggi con un observer API
        /// </summary>
        [Fact]
        public void ObserveFlyingMission()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            var config = SystemConfigs.DroneConfig;

            // creo il registro
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository");

            // creo il tool per lo spawn di missioni
            MissionSpawner missionSpawner = new MissionSpawner(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            // spawno una missione
            IMissionAPI a = missionSpawner.SpawnHere(missionA, "DroneA")!;
            Assert.IsType<ObserverMissionAPI>(a);

            ObserverMissionAPI obsAPI = (ObserverMissionAPI) a;

            // ricevo tutte le notifiche
            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI.AskForUpdates().Result;

                foreach(var n in newNotifications)
                {
                    notifications.Add(n);
                }

            } // while (notifications.Last() is ExitStateDTO);
            while (notifications.Count < 4);

            // mi assicuro di aver ricevuto le notifiche
            // che mi aspettavo (nell'ordine giusto)
            Assert.IsType<InitStateDTO>(notifications[0]);
            Assert.IsType<NegotiateStateDTO>(notifications[1]);
            Assert.IsType<FlyingStateDTO>(notifications[2]);
            Assert.IsType<ExitStateDTO>(notifications[3]);

            Sys.Terminate();
        }

        /// <summary>
        /// Crea un'API per connetterti ad una missione già esistente.
        /// Uso l'API per osservarla.
        /// </summary>
        [Fact]
        public void ObserveExistingMission()
        {         
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            var config = SystemConfigs.DroneConfig;
            config.SystemName = "test";

            // creo il registro
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository"); ;

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new MissionSpawner(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            // spawno una missione
            var spawnerActor = Sys.ActorOf<SpawnerActor>("spawner");
            spawnerActor.Ask(new SpawnActorRequest(DroneActor.Props(register, missionA), "DroneA")).Wait();


            // creo una seconda API per l'osservazione
            ObserverMissionAPI? obsAPI = (ObserverMissionAPI?) new MissionProvider(Sys, config, ObserverMissionAPI.Factory(Sys))
                .TryConnectToExistent(Host.GetTestHost(), "DroneA");

            // ricevo tutte le notifiche
            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI!.AskForUpdates().Result;

                foreach (var n in newNotifications)
                {
                    notifications.Add(n);
                }

            } // while (notifications.Last() is ExitStateDTO);
            while (notifications.Count < 4);

            // mi assicuro di aver ricevuto le notifiche
            // che mi aspettavo (nell'ordine giusto)
            Assert.IsType<InitStateDTO>(notifications[0]);
            Assert.IsType<NegotiateStateDTO>(notifications[1]);
            Assert.IsType<FlyingStateDTO>(notifications[2]);
            Assert.IsType<ExitStateDTO>(notifications[3]);

            Sys.Terminate();
        }
        #endregion

        #region test retrocompatibilità 
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
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository"); ;

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new MissionSpawner(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

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

            var config = SystemConfigs.DroneConfig;
            config.SystemName = "test";

            // creo il registro
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository"); ;

            // spawno una missione manualmente
            var spawnerActor = Sys.ActorOf<SpawnerActor>("spawner");
            spawnerActor.Ask(new SpawnActorRequest(DroneActor.Props(register, missionA), "DroneA")).Wait();

            // uso il tool per ricavare un'istanza dell'API e le richiedo lo stato
            IMissionAPI? a = new MissionProvider(Sys, config).TryConnectToExistent(Host.GetTestHost(), "DroneA");
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
            config.SystemName = Sys.Name;

            // creo il registro
            IActorRef register = Sys.ActorOf(
                DronesRepositoryActor.Props(),
                "repository"); ;

            // uso il tool per spawnare in remoto una missione
            // e ricavare un'istanza dell'API
            MissionSpawner spawner = new MissionSpawner(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            _ = Sys.ActorOf<SpawnerActor>("spawner");

            IMissionAPI a = spawner.SpawnRemote(Host.GetTestHost(), missionA, "DroneA")!;
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);

            Sys.Terminate();
        }
        #endregion
    }*/
}
