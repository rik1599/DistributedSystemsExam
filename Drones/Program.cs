using Akka.Actor;

using (var system = ActorSystem.Create("DeployTarget")) 
{
    Console.ReadKey();
}