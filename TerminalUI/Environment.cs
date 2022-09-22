using Akka.Actor;
using Akka.Actor.Internal;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;
using DroneSystemAPI.APIClasses.Repository;

namespace TerminalUI
{
    internal class Environment
    {
        public IDictionary<int, ActorSystem> ActorSystems { get; }
        public ActorSystem InterfaceActorSystem { get; }
        public RepositoryAPI? RepositoryAPI { get; set; }
        public IDictionary<string, Host> GeneratedMissions { get; }
        public IDictionary<string, ObserverMissionAPI> ConnectedMissions { get; }

        public Environment()
        {
            ActorSystems = new Dictionary<int, ActorSystem>();

            var config = SystemConfigs.GenericConfig;
            config.SystemName = "InterfaceActorSystem";
            config.Port = 0;

            InterfaceActorSystem = ActorSystem.Create(config.SystemName, config.Config);
            GeneratedMissions = new Dictionary<string, Host>();
            ConnectedMissions = new Dictionary<string, ObserverMissionAPI>();
        }

        public void Terminate()
        {
            InterfaceActorSystem?.Terminate();
            foreach (var system in ActorSystems)
            {
                ActorSystems.Remove(system);
                system.Value.Terminate();
            }
        }
    }

    internal class ActorSystemFactory
    {
        public static ActorSystem? CreateActorSystem(Environment env, SystemConfigs config, out int port)
        {
            ActorSystemImpl? system;
            try
            {
                system = ActorSystem.Create(config.SystemName, config.Config) as ActorSystemImpl;
                var assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port;
                env.ActorSystems.Add(assignedPort!.Value, system);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"ActorSystem creato alla porta {assignedPort}");
                port = assignedPort.Value;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Porta già utilizzata!");
                system = null;
                port = 0;
            }
            return system;
        }
    }
}
