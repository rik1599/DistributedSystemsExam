using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Configuration;
using DroneSystemAPI;
using TerminalUI.Verbs;

namespace TerminalUI
{
    internal class Environment
    {
        public IDictionary<int, ActorSystemRolePair> ActorSystems { get; }
        public ActorSystem InterfaceActorSystem { get; }

        public Environment()
        {
            ActorSystems = new Dictionary<int, ActorSystemRolePair>();

            var config = SystemConfigs.DroneConfig;
            config.SystemName = "InterfaceActorSystem";
            config.Port = 0;

            InterfaceActorSystem = ActorSystem.Create(config.SystemName, config.Config);
        }

        public void Terminate()
        {
            var tasks = new List<Task>
            {
                InterfaceActorSystem.WhenTerminated
            };

            foreach (var system in ActorSystems)
            {
                ActorSystems.Remove(system);
                tasks.Add(system.Value.ActorSystem.WhenTerminated);
            }
            Task.WhenAll(tasks).RunSynchronously();
        }
    }

    internal class ActorSystemRolePair
    {
        public ActorSystem ActorSystem { get; }
        public ActorSystemRole Role { get; set; }

        public ActorSystemRolePair(ActorSystem actorSystem)
        {
            ActorSystem = actorSystem;
            Role = ActorSystemRole.None;
        }
    }

    internal enum ActorSystemRole
    {
        None,
        Drone,
        Repository
    }

    internal class ActorSystemFactory
    {
        public static ActorSystem? CreateActorSystem(Environment env, SystemConfigs config)
        {
            ActorSystemImpl? system;
            try
            {
                system = ActorSystem.Create(config.SystemName, config.Config) as ActorSystemImpl;
                var assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port;
                env.ActorSystems.Add(assignedPort!.Value, new ActorSystemRolePair(system));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"ActorSystem creato alla porta {assignedPort}");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Porta già utilizzata!");
                system = null;
            }
            Console.ForegroundColor = ConsoleColor.White;
            return system;
        }
    }
}
