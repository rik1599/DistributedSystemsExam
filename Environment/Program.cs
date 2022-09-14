using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.Configuration;
using Environment.Actor;
using Environment.PhisicalHost;
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

var droneSystem = ActorSystem.Create("Drone", ConfigurationFactory.ParseString(@"
akka {  
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8081
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
"));


Console.WriteLine("Creazione attore ambiente.");


var environment = envSystem.ActorOf(Props.Create(() => new EnvironmentActor()));

Console.ReadKey();
Console.WriteLine("Spawn missione.");


environment.Tell(new SpawnMissionRequest(
    new MissionPath(Point2D.Origin, new Point2D(10, 30), 10f),
    new HostFactoryTCP().Create("localhost", 8081)
    ));

Console.ReadKey();

envSystem.Terminate();
