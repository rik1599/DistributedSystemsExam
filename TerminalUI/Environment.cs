using Akka.Actor;
using Akka.Actor.Internal;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses.Repository;

namespace TerminalUI
{
    internal class Environment
    {
        public IDictionary<int, ActorSystem> ActorSystems { get; }
        public ActorSystem InterfaceActorSystem { get; }
        public IActorRef? RepositoryAPI { get; set; }
        public IDictionary<string, MissionInfo> Missions { get; }
        public Environment()
        {
            ActorSystems = new Dictionary<int, ActorSystem>();

            var config = SystemConfigs.GenericConfig;
            config.SystemName = "InterfaceActorSystem";
            config.Port = 0;

            InterfaceActorSystem = ActorSystem.Create(config.SystemName, config.Config);
            Missions = new Dictionary<string, MissionInfo>();
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
