using Actors;
using Actors.Messages.Register;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Utils;

namespace UnitTests.API.Deprecated.Components
{
    
    /*
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
            DeployPointInitializer initializer = new DeployPointInitializer(Sys);

            var remoteLocation = new RemoteLocationAPI(Sys, DeployPointDetails.GetTestDetails());

            Assert.True(!remoteLocation.Verify());
            initializer.Init();
            Assert.True(remoteLocation.Verify());
        }


        /// <summary>
        /// Spawna e prova a contattare l'attore
        /// </summary>
        [Fact]
        public void RemoteSpawnThroughAPITest()
        {
            new DeployPointInitializer(Sys).Init();

            var remoteLocation = new RemoteLocationAPI(Sys, DeployPointDetails.GetTestDetails());

            var echoActorRef = remoteLocation
                .SpawnActor(EchoTestActor.Props(), "echo");

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
            new DeployPointInitializer(Sys).Init();

            IActorRef spawner = Sys.ActorSelection(Host.GetTestHost().GetActorAddress("test", "spawner"))
                .ResolveOne(new TimeSpan(0, 0, 5)).Result;

            var remoteLocation = new RemoteLocationAPI(Sys, DeployPointDetails.GetTestDetails());

            _ = remoteLocation.SpawnActor(EchoTestActor.Props(), "echo");

            var echoActorRef = remoteLocation.GetActorRef("echo");

            Assert.IsAssignableFrom<IActorRef>(echoActorRef);

            var res = echoActorRef.Ask<String>("hello world").Result;
            Assert.Equal("hello world", res);
        }
    }*/
}
