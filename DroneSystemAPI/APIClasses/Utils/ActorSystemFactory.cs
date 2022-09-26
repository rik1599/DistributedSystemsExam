using Actors;
using Akka.Actor;
using Akka.Actor.Internal;

namespace DroneSystemAPI.APIClasses.Utils
{
    public static class ActorSystemFactory
    {
        public static ActorSystem Create(int port, out int assignedPort)
        {
            var config = SystemConfigs.GenericConfig;
            config.Port = port;

            ActorSystemImpl? system;
            try
            {
                system = ActorSystem.Create(config.SystemName, config.Config) as ActorSystemImpl;
                assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port!.Value;
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
