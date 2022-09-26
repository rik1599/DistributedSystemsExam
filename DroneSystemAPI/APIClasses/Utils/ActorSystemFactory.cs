using Actors;
using Akka.Actor;
using Akka.Actor.Internal;

namespace DroneSystemAPI.APIClasses.Utils
{
    public static class ActorSystemFactory
    {
        public static ActorSystem Create(out int port)
        {
            var config = SystemConfigs.GenericConfig;

            ActorSystemImpl? system;
            try
            {
                system = ActorSystem.Create(config.SystemName, config.Config) as ActorSystemImpl;
                var assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port;
                port = assignedPort!.Value;
                system.ActorOf(Props.Create(() => new SpawnerActor()), config.SpawnerActor);
            }
            catch (Exception)
            {
                throw;
            }

            return system;
        }
    }
}
