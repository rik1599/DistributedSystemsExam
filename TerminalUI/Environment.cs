using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Configuration;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;

namespace TerminalUI
{
    internal class Environment
    {
        public IDictionary<int, ActorSystem> ActorSystems { get; }

        public ActorSystem InterfaceActorSystem { get; }

        public IDictionary<string, MissionInfo> Missions { get; }

        /// <summary>
        /// API principale per coordinare il sistema (da "client")
        /// </summary>
        public DroneDeliverySystemAPI DroneDeliverySystemAPI { get; }

        public Environment()
        {
            // inizializzo actor system locale usato a scopo di interfaccia
            InterfaceActorSystem = ActorSystem.Create(
                "InterfaceActorSystem",
                ConfigurationFactory.ParseString(_interfaceActorSystemHookon()));

            // inizializzo le liste degli actor system gestiti localmente
            ActorSystems = new Dictionary<int, ActorSystem>();
            Missions = new Dictionary<string, MissionInfo>();

            // inizializzo API
            DroneDeliverySystemAPI = new DroneDeliverySystemAPI(
                InterfaceActorSystem,
                Config2.Default().SystemName,
                Config2.Default().RepositoryActorName
                );
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

        private static string _interfaceActorSystemHookon()
        {
            return @$"
akka {{
    loglevel = WARNING
    actor {{
        provider = remote
    }}
    remote {{
        dot-netty.tcp {{
            port = 0
            hostname = localhost
        }}
    }}
}}";
        }


    }
}
