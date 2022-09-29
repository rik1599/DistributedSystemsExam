using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;

namespace UnitTests.API.Deprecated
{
    /*
    public class RepositoryAPITest : TestKit
    {
        /// <summary>
        /// Creo un registro in locale usando le API
        /// </summary>
        [Fact]
        public void SpawnRegisterLocally()
        {
            var config = SystemConfigs.RepositoryConfig;
            config.SystemName = Sys.Name;

            // spawn del registro usando il provider
            var registerProvider = new RepositoryProvider(Sys, config);
            RepositoryAPI register = registerProvider.SpawnHere()!;

            Assert.NotNull(register);
            CheckRegister(register.ActorRef);

            Sys.Terminate();
        }

        [Fact]
        public void SpawnRegisterRemote()
        {
            var config = SystemConfigs.RepositoryConfig;
            config.SystemName = Sys.Name;

            new DeployPointInitializer(Sys).Init();

            // spawn del registro (in remoto) usando il provider
            var registerProvider = new RepositoryProvider(Sys, config);
            IActorRef? register = registerProvider.SpawnRemote(Host.GetTestHost());

            Assert.NotNull(register);
            CheckRegister(register!);

            Sys.Terminate();
        }

        [Fact]
        public void ConnectToExistentRegister()
        {
            var config = SystemConfigs.RepositoryConfig;
            config.SystemName = Sys.Name;

            // spawn del registro
            var spawner = Sys.ActorOf<SpawnerActor>("spawner");
            spawner.Ask(new SpawnActorRequest(DronesRepositoryActor.Props(), config.ActorName)).Wait();

            // connessione (usando il provider)
            var registerProvider = new RepositoryProvider(Sys, config);
            IActorRef? register = registerProvider.TryConnectToExistent(Host.GetTestHost());

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
    }*/
}
