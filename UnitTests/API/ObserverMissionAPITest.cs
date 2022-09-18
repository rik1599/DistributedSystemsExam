using Actors;
using Akka.TestKit.Xunit2;
using Akka.Actor;
using Actors.MissionPathPriority;
using MathNet.Spatial.Euclidean;
using Actors.DTO;
using DroneSystemAPI.APIClasses.Register;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;

namespace UnitTests.API
{
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

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RepositoryAPI register = new RepositoryProvider(Sys).SpawnHere();

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            // spawno una missione
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA");
            Assert.IsType<ObserverMissionAPI>(a);

            ObserverMissionAPI obsAPI = (ObserverMissionAPI) a;

            // ricevo tutte le notifiche
            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI.AskForNotifications().Result;

                foreach(var n in newNotifications)
                {
                    notifications.Add(n);
                }

            } // while (notifications.Last() is ExitStateDTO);
            while (notifications.Count() < 4);

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

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RepositoryAPI register = new RepositoryProvider(Sys).SpawnHere();

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            // spawno una missione
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA");

            // creo una seconda API per l'osservazione
            ObserverMissionAPI? obsAPI = (ObserverMissionAPI?) spawner
                .TryConnectToExistent(Host.GetTestHost(), "DroneA");

            // ricevo tutte le notifiche
            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI.AskForNotifications().Result;

                foreach (var n in newNotifications)
                {
                    notifications.Add(n);
                }

            } // while (notifications.Last() is ExitStateDTO);
            while (notifications.Count() < 4);

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

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RepositoryAPI register = new RepositoryProvider(Sys).SpawnHere();

            // creo il tool per lo spawn di missioni
            MissionSpawner spawner = new(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            // spawno due missioni 
            IMissionAPI a = spawner.SpawnHere(missionA, "DroneA");
            IMissionAPI b = spawner.SpawnHere(missionB, "DroneB");

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

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RepositoryAPI register = new RepositoryProvider(Sys).SpawnHere();

            // spawno una missione manualmente
            var realRef = Sys.ActorOf(
                DroneActor.Props(register.ActorRef, missionA)
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(
                        Address.Parse(Host.GetTestHost().GetSystemAddress(config.DroneSystemName))
                        ))),
                "DroneA");


            // uso il tool per ricavare un'istanza dell'API e le richiedo lo stato
            MissionSpawner spawner = new(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            IMissionAPI? a = spawner.TryConnectToExistent(Host.GetTestHost(), "DroneA");
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

            DroneSystemConfig config = new()
            {
                RegisterSystemName = "test",
                DroneSystemName = "test"
            };

            // creo il registro
            RepositoryAPI register = new RepositoryProvider(Sys).SpawnHere();

            // uso il tool per spawnare in remoto una missione
            // e ricavare un'istanza dell'API
            MissionSpawner spawner = new(Sys,
                register, ObserverMissionAPI.Factory(Sys), config);

            IMissionAPI a = spawner.SpawnRemote(Host.GetTestHost(), missionA, "DroneA");
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);

            Sys.Terminate();
        }


        #endregion
    }
}
