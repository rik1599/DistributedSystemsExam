using Actors;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Configuration;
using Environment;
using MathNet.Spatial.Euclidean;

Console.WriteLine("Creazione ambiente deployer e drone.");

var envSystem = ActorSystem.Create("Deployer", ConfigurationFactory.ParseString(@"
akka {  
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8080
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
"));

Console.WriteLine("Creazione attore ambiente.");
var repository = envSystem.ActorOf(DronesRepositoryActor.Props());

var droneConfiguration = new Actors.Config();
List<ActorSystem> drones = new();
for (int i = 0; i < 1; i++)
{
    var droneSystem = ActorSystem.Create(droneConfiguration.ActorSystemName, droneConfiguration.DroneConfig);
    drones.Add(droneSystem);

    var mission = new MissionPath(Point2D.Origin, new Point2D(10, 30), 10f);
    droneSystem.ActorOf(RegisterDroneActor.Props(repository, mission));

    Console.ReadKey();
}

foreach (var drone in drones)
{
    drone.Terminate();
}
envSystem.Terminate();
