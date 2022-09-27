using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;

namespace UnitTests.API
{
    class EchoTestActor : UntypedActor
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
    /// Test dei componenti che si usano per fare il deployment.
    /// </summary>
    public class DeployAPITest : TestKit
    {
        
        
        /// <summary>
        /// Verifica se spawno correttamente un attore spawner
        /// in una locazione. 
        /// </summary>
        [Fact]
        public void SetupLocationTest()
        {
            LocationInitializerAPI initializer = new LocationInitializerAPI(Sys);

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
            new LocationInitializerAPI(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner"))
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
            new LocationInitializerAPI(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner"))
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
            new LocationInitializerAPI(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner"))
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            spawner.Ask<IActorRef>(
                new SpawnActorRequest(
                    EchoTestActor.Props(),
                    "echo"
                )).Wait();

            var echoActorRef = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner/echo"))
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }
    }
}
