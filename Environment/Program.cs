using Akka.Actor;
using Environment;
using MathNet.Spatial.Euclidean;

using (var system = ActorSystem.Create("Deployer"))
{
    var environment = system.ActorOf(Props.Create(() => new EnvironmentActor()));

    Mission mission = new(Point2D.Origin, new Point2D(10, 30));
    Host host = new HostFactoryTCP().Create("localhost", 8081);
    MissionMessage message = new(mission, host.Parse());

    Console.ReadKey();

    environment.Tell(message);

    Console.ReadKey();
    system.Terminate();
}