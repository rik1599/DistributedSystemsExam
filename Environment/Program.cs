using Actors.MissionPathPriority;
using Akka.Actor;
using Environment;
using Environment.Actor;
using Environment.PhisicalHost;
using MathNet.Spatial.Euclidean;

using (var system = ActorSystem.Create("Deployer"))
{
    var environment = system.ActorOf(Props.Create(() => new EnvironmentActor()));

    MissionPath missionPath = new MissionPath(Point2D.Origin, new Point2D(10, 30), 10f);

    Host host = new HostFactoryTCP().Create("localhost", 8081);
    SpawnMissionRequest message = new SpawnMissionRequest(missionPath, host);

    Console.ReadKey();

    environment.Tell(message);

    Console.ReadKey();
    system.Terminate();
}