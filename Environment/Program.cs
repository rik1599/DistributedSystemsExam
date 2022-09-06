using Akka.Actor;
using Actors;
using Environment;

using (var system = ActorSystem.Create("Deployer"))
{
    var remoteAddress = Address.Parse("akka.tcp://DeployTarget@localhost:8081");
    var remoteEcho1 = system.ActorOf(
        Props.Create(() => new EchoActor())
            .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))), 
        "remoteecho1");
    var remoteEcho2 = system.ActorOf(
        Props.Create(() => new EchoActor())
            .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))),
        "remoteecho2");

    Console.ReadKey();

    system.ActorOf(Props.Create(() => new HelloActor(remoteEcho1)));
    system.ActorOf(Props.Create(() => new HelloActor(remoteEcho2)));

    Console.ReadKey();
}