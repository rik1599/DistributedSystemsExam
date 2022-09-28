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
    
    /// <summary>
    /// Test dei componenti che si usano per fare il deployment.
    /// </summary>
    public class DeployAPITest : TestKit
    {
        private class EchoTestActor : UntypedActor
        {
            protected override void OnReceive(object message)
            {
                Sender.Tell(message);
            }

            public static Props Props()
            {
                return Akka.Actor.Props.Create(() => new EchoTestActor());
            }
        }

        /// <summary>
        /// Verifica se spawno correttamente un attore spawner
        /// in una locazione. 
        /// </summary>
        [Fact]
        public void SetupLocationTest()
        {
            DeployPointInitializer initializer = new DeployPointInitializer(Sys);

            Assert.True(!initializer.IsInitialized());
            initializer.Init();
            Assert.True(initializer.IsInitialized());
        }

        /// <summary>
        /// Inizializza una locazione e verifica l'esistenza dello spawner
        /// </summary>
        [Fact]
        public void CheckSpawnerTest()
        {
            new DeployPointInitializer(Sys).Init();

            String spawnerAddress = DeployPointDetails.GetTestDetails().SpawnerAddress();
            Assert.Equal("akka://test/user/spawner", spawnerAddress);

            IActorRef spawner = Sys.ActorSelection(spawnerAddress)
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var res = spawner.Ask(new SpawnActorTestMessage()).Result;
            Assert.IsType<bool>(res);
            Assert.True((bool) res);
        }

        /// <summary>
        /// Spawna  e prova a contattare l'attore
        /// </summary>
        [Fact]
        public void RemoteSpawnTest()
        {
            new DeployPointInitializer(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(DeployPointDetails.GetTestDetails().SpawnerAddress())
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var echoActorRef = spawner.Ask<IActorRef>(
                new SpawnActorRequest(
                    EchoTestActor.Props(), 
                    "echo"
                )).Result;

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }

        /// <summary>
        /// Spawna tramite API remote e ottieni attore tramite actor selection
        /// </summary>
        [Fact]
        public void RemoteSpawnReachabilityTest()
        {
            new DeployPointInitializer(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(DeployPointDetails.GetTestDetails().SpawnerAddress())
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            spawner.Ask<IActorRef>(
                new SpawnActorRequest(
                    EchoTestActor.Props(),
                    "echo"
                )).Wait();

            var echoActorRef = Sys.ActorSelection(DeployPointDetails.GetTestDetails().SpawnerAddress() + "/echo")
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }
    } */
}
