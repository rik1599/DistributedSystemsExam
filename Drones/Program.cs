using Akka.Actor;

using (var system = ActorSystem.Create(Actors.Config.ActorSystemName))
{
    Console.ReadKey();
    system.Terminate();
}