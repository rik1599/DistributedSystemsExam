using Akka.Actor;
using Akka.Actor.Internal;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;

namespace TerminalUI
{
    internal class Environment
    {
        public IDictionary<int, ActorSystem> ActorSystems { get; }
        public ActorSystem InterfaceActorSystem { get; }
        public IActorRef? RepositoryAPI { get; set; }
        public IDictionary<string, MissionInfo> Missions { get; }

        /// <summary>
        /// 
        /// </summary>
        public DroneDeliverySystemAPI DroneDeliverySystemAPI { get; }

        public Environment()
        {
            // inizializzo actor system locale usato a scopo di interfaccia
            var config = SystemConfigs.GenericConfig;
            config.SystemName = "InterfaceActorSystem";
            config.Port = 0;
            InterfaceActorSystem = ActorSystem.Create(config.SystemName, config.Config);

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
    }
}
