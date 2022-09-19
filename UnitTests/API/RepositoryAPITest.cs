using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Register;

namespace UnitTests.API
{
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
            RepositoryAPI register = registerProvider.SpawnHere();

            Assert.NotNull(register);
            CheckRegister(register);

            Sys.Terminate();
        }

        [Fact]
        public void SpawnRegisterRemote()
        {
            var config = SystemConfigs.RepositoryConfig;
            config.SystemName = Sys.Name;

            // spawn del registro (in remoto) usando il provider
            var registerProvider = new RepositoryProvider(Sys, config);
            RepositoryAPI register = registerProvider.SpawnRemote(Host.GetTestHost());

            Assert.NotNull(register);
            CheckRegister(register);

            Sys.Terminate();
        }

        [Fact]
        public void ConnectToExistentRegister()
        {
            var config = SystemConfigs.RepositoryConfig;
            config.SystemName = Sys.Name;

            // spawn del registro (in remoto)
            _ = Sys.ActorOf(
                DronesRepositoryActor.Props()
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(
                        Address.Parse(Host.GetTestHost().GetSystemAddress(config.SystemName))
                        ))),
                config.ActorName);

            // connessione (usando il provider)
            var registerProvider = new RepositoryProvider(Sys, config);
            RepositoryAPI? register = registerProvider.TryConnectToExistent(Host.GetTestHost());

            Assert.NotNull(register);
            CheckRegister(register!);

            Sys.Terminate();
        }

        /// <summary>
        /// Controlla se il registro funziona
        /// </summary>
        /// <param name="register"></param>
        private void CheckRegister(RepositoryAPI register)
        {
            register.ActorRef.Tell(new RegisterRequest(TestActor), TestActor);
            ExpectMsgFrom<RegisterResponse>(register.ActorRef);
        } 
    }
}
