using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Utils;

namespace UnitTests.API
{
    

    /// <summary>
    /// Test dei componenti che si usano per fare il deployment.
    /// </summary>
    public class RemoteLocationAPITest : TestKit
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
        /// in una locazione (tramite API remote)
        /// </summary>
        [Fact]
        public void CheckSpawnerThroughAPITest()
        {
            LocationInitializerAPI initializer = new LocationInitializerAPI(Sys);

            var remoteLocation = new RemoteLocationAPI();

            Assert.True(true);
            initializer.Init();
            Assert.True(true);
        }


        /// <summary>
        /// Spawna e prova a contattare l'attore
        /// </summary>
        [Fact]
        public void RemoteSpawnThroughAPITest()
        {
            new LocationInitializerAPI(Sys).Init();

            var remoteLocation = new RemoteLocationAPI();

            var echoActorRef = remoteLocation.SpawnRemote(
                Sys, 
                Address.Parse(Host.GetTestHost().GetSystemAddress("test")), 
                EchoTestActor.Props(), 
                "echo"
                );

            Assert.IsAssignableFrom<IActorRef>(echoActorRef);

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }

        /// <summary>
        /// Spawna tramite API remote e ottieni attore tramite actor selection
        /// </summary>
        [Fact]
        public void RemoteGetRefThroughAPITest()
        {
            new LocationInitializerAPI(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner"))
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var remoteLocation = new RemoteLocationAPI();

            _ = remoteLocation.SpawnRemote(
                Sys,
                Address.Parse(Host.GetTestHost().GetSystemAddress("test")),
                EchoTestActor.Props(),
                "echo"
                );

            var echoActorRef = remoteLocation.TryGetExistentActor(
                Sys,
                Address.Parse(Host.GetTestHost().GetSystemAddress("test")),
                "echo"
                );

            Assert.IsAssignableFrom<IActorRef>(echoActorRef);

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }
    }
}
