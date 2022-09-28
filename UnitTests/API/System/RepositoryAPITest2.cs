using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Utils;

namespace UnitTests.API.System
{
    public class RepositoryAPITest2 : TestKit
    {
        private readonly DeployPointInitializer _remoteLocationInitializer;

        private readonly string _systemName; 
        private readonly string _repositoryActorName;

        public RepositoryAPITest2()
        {
            _systemName = Sys.Name;
            _repositoryActorName = "repository";

            _remoteLocationInitializer = new DeployPointInitializer(Sys);
            _remoteLocationInitializer.Init();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Sys.Terminate();
        }

        /// <summary>
        /// Avvia un repository e verifica il suo corretto funzionamento
        /// </summary>
        [Fact]
        public void SpawnRepository()
        {
            var api = new DroneDeliverySystemAPI(Sys, _systemName, _repositoryActorName);

            api.DeployRepository(Host.GetTestHost());

            IActorRef? register = api.RepositoryAddress;

            Assert.NotNull(register);
            CheckRegister(register!);
        }


        /// <summary>
        /// Connettiti ad un registro che esite già 
        /// </summary>
        [Fact]
        public void ConnectToExistentRegister()
        {
            // spawn a parte del registro
            RemoteLocationAPI remoteLocationAPI = new RemoteLocationAPI(
                Sys, DeployPointDetails.GetTestDetails());
            remoteLocationAPI.SpawnActor(DronesRepositoryActor.Props(), _repositoryActorName);

            // connessione (usando le API)

            var api = new DroneDeliverySystemAPI(Sys, _systemName, _repositoryActorName);

            api.SetRepository(Host.GetTestHost());

            IActorRef? register = api.RepositoryAddress;

            Assert.NotNull(register);
            CheckRegister(register!);

            Sys.Terminate();
        }

        /// <summary>
        /// Controlla se il registro funziona
        /// </summary>
        /// <param name="register"></param>
        private void CheckRegister(IActorRef register)
        {
            register.Tell(new RegisterRequest(TestActor), TestActor);
            ExpectMsgFrom<RegisterResponse>(register);
        } 
    }
}
