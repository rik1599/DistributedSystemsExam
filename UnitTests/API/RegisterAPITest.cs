using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.Configuration;
using Akka.TestKit.Xunit2;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Register;
using DroneSystemAPI.APIClasses.Utils;
using Environment;

namespace UnitTests.API
{
    public class RegisterAPITest : TestKit
    {
        /// <summary>
        /// Creo un registro in locale usando le API
        /// </summary>
        [Fact]
        public void SpawnRegisterLocally()
        {
            var config = new DroneSystemConfig();
            config.RegisterSystemName = Sys.Name;

            // spawn del registro usando il provider
            var registerProvider = new RegisterProvider(Sys, config);
            RegisterAPI register = registerProvider.SpawnHere();

            Assert.NotNull(register);
            _checkRegister(register);

            Sys.Terminate();
        }

        [Fact]
        public void SpawnRegisterRemote()
        {
            var config = new DroneSystemConfig();
            config.RegisterSystemName = Sys.Name;

            // spawn del registro (in remoto) usando il provider
            var registerProvider = new RegisterProvider(Sys, config);
            RegisterAPI register = registerProvider.SpawnRemote(Host.GetTestHost());

            Assert.NotNull(register);
            _checkRegister(register);

            Sys.Terminate();
        }

        [Fact]
        public void ConnectToExistentRegister()
        {
            var config = new DroneSystemConfig();
            config.RegisterSystemName = Sys.Name;

            // spawn del registro (in remoto)
            var realRef = Sys.ActorOf(
                DronesRepositoryActor.Props()
                    .WithDeploy(Deploy.None.WithScope(new RemoteScope(
                        Address.Parse(Host.GetTestHost().GetSystemAddress(config.RegisterSystemName))
                        ))),
                config.RegisterActorName);

            // connessione (usando il provider)
            var registerProvider = new RegisterProvider(Sys, config);
            RegisterAPI? register = registerProvider.TryConnectToExistent(Host.GetTestHost());

            Assert.NotNull(register);
            _checkRegister(register!);

            Sys.Terminate();
        }

        /// <summary>
        /// Controlla se il registro funziona
        /// </summary>
        /// <param name="register"></param>
        private void _checkRegister(RegisterAPI register)
        {
            register.ActorRef.Tell(new RegisterRequest(TestActor), TestActor);
            ExpectMsgFrom<RegisterResponse>(register.ActorRef);
        } 
    }
}
