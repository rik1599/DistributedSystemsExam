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
using DroneSystemAPI.APIClasses.Utils;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;

namespace UnitTests.API.System
{
    /// <summary>
    /// Rappresentazione di situazioni nelle quali un nuovo nodo 
    /// spawna e deve gestirsi dei semplici conflitti (con un unico
    /// altro nodo).
    /// </summary>
    public class ObserverMissionAPITest2 : TestKit
    {
        private readonly string _systemName;
        private readonly string _repositoryActorName;

        private DroneDeliverySystemAPI _api;

        IMissionAPIFactory _factory;

        public ObserverMissionAPITest2()
        {
            _systemName = Sys.Name;
            _repositoryActorName = "repository";

            Sys.ActorOf(Props.Create(() => new SpawnerActor()), "spawner");

            // var register = Sys.ActorOf(DronesRepositoryActor.Props(), "repository");

            _api = new DroneDeliverySystemAPI(Sys, _systemName, _repositoryActorName);
            _api.DeployRepository(Host.GetTestHost());

            _factory = ObserverMissionAPI.Factory(Sys);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Sys.Terminate();
        }

        #region verifica observer
        /// <summary>
        /// Avvio di una missione (in locale) e monitoraggio 
        /// dei vari passaggi con un observer API
        /// </summary>
        [Fact]
        public void ObserveFlyingMission()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            // spawno una missione
            IMissionAPI a = _api.SpawnMission(
                Host.GetTestHost(),
                missionA,
                "DroneA",
                _factory);
            
            Assert.IsType<ObserverMissionAPI>(a);

            ObserverMissionAPI obsAPI = (ObserverMissionAPI)a;

            // ricevo tutte le notifiche
            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI.AskForUpdates().Result;

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
        }

        /// <summary>
        /// Crea un'API per connetterti ad una missione già esistente.
        /// Uso l'API per osservarla.
        /// </summary>
        [Fact]
        public void ObserveExistingMission()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            // spawno una missione manualmente
            RemoteLocationAPI remoteLocationAPI = new RemoteLocationAPI(
                Sys, DeployPointDetails.GetTestDetails());

            remoteLocationAPI.SpawnActor(
                DroneActor.Props(_api.RepositoryAddress!, missionA),
                "DroneA");

            // creo una seconda API per l'osservazione
            ObserverMissionAPI? obsAPI = (ObserverMissionAPI?) 
                _api.ConnectToMission(Host.GetTestHost(), "DroneA", _factory);

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

        #region test di retro-compatibilità

        /// <summary>
        /// Semplice tentativo di avvio di una missione 
        /// (e richiesta stato) tramite le API
        /// </summary>
        [Fact]
        public void MissionStart()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            // spawno due missioni 
            IMissionAPI a = _api.SpawnMission(
                Host.GetTestHost(), 
                missionA, 
                "DroneA",
                _factory);

            IMissionAPI b = _api.SpawnMission(
                Host.GetTestHost(),
                missionB,
                "DroneB",
                _factory);

            // richiedo ad entrambe lo stato
            var aStatus = a.GetCurrentStatus();
            Assert.IsAssignableFrom<Task<DroneStateDTO>>(aStatus);
            Assert.IsAssignableFrom<DroneStateDTO>(aStatus.Result);
            Assert.IsAssignableFrom<DroneStateDTO>(b.GetCurrentStatus().Result);
        }

        /// <summary>
        /// Crea un'API per connetterti ad una missione già esistente.
        /// </summary>
        [Fact]
        public void ConnectToExistingMission()
        {         
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);


            // spawno una missione manualmente
            RemoteLocationAPI remoteLocationAPI = new RemoteLocationAPI(
                Sys, DeployPointDetails.GetTestDetails());

            remoteLocationAPI.SpawnActor(
                DroneActor.Props(_api.RepositoryAddress!, missionA), 
                "DroneA");

            // mi ci collego
            IMissionAPI a = _api.ConnectToMission(Host.GetTestHost(), "DroneA", SimpleMissionAPI.Factory());
            
            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);
        }

        /// <summary>
        /// Spawno una missione, la faccio volare e la annullo.
        /// </summary>
        [Fact]
        public void CancelMission()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);
            var missionB = new MissionPath(new Point2D(25, 0), new Point2D(0, 25), 10.0f);

            // mi iscrivo al registro
            _api.RepositoryAddress.Tell(new RegisterRequest(TestActor));
            ExpectMsgFrom<RegisterResponse>(_api.RepositoryAddress);

            // spawno una missione
            IMissionAPI a = _api.SpawnMission(
                Host.GetTestHost(),
                missionA,
                "DroneA",
                _factory);

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
        }

        /// <summary>
        /// mi collego ad una missione che non esiste e verifico che lanci un'eccezione
        /// </summary>
        [Fact]
        public void ConnectToUnexistentMission()
        {
            Assert.ThrowsAny<Exception>(
                () => _api.ConnectToMission(
                    Host.GetTestHost(), "Drone746", _factory)
            );
        }

        /// <summary>
        /// provo a spawnare una missione senza aver creato/impostato un registro
        /// </summary>
        [Fact]
        public void SpawnWithoutRegister()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            var api2 = new DroneDeliverySystemAPI(Sys, _systemName, _repositoryActorName);

            Assert.ThrowsAny<Exception>(
                () => api2.SpawnMission(
                    Host.GetTestHost(), missionA, "DroneA", _factory)
            );

            // riprovo e mi aspetto che vada
            api2.SetRepository(Host.GetTestHost());
            Assert.IsAssignableFrom<IMissionAPI>(api2.SpawnMission(
                    Host.GetTestHost(), missionA, "DroneA", _factory));

        }

        /// <summary>
        /// provo a connettermi ad una missione senza aver creato/impostato un registro
        /// Questa volta dovrebbe funzionare.
        /// </summary>
        [Fact]
        public void ConnectionWithoutRegister()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(100, 100), 10.0f);

            // spawno la missione
            _api.SpawnMission(Host.GetTestHost(), 
                missionA, "DroneA", _factory);

            // provo a connettermi senza impostare il registro
            var api2 = new DroneDeliverySystemAPI(Sys, _systemName, _repositoryActorName);

            IMissionAPI a = api2.ConnectToMission(
                Host.GetTestHost(), "DroneA", _factory);

            Assert.NotNull(a);
            Assert.IsAssignableFrom<DroneStateDTO>(a!.GetCurrentStatus().Result);
        }

        #endregion
    }
}
