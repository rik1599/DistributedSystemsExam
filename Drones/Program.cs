using Akka.Actor;

using (var system = ActorSystem.Create(Actors.Constants.ActorSystemName))
{
    Console.ReadKey();
    system.Terminate();
}