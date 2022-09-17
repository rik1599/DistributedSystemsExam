using Akka.Actor;

namespace DroneSystemAPI.APIClasses.Utils
{
    /// <summary>
    /// Strumento unico per:
    /// - reperire facilmente gli indirizzi di attori collocati su sistemi noti
    /// - spawnare attori (localmente o su sistemi remoti avviati)
    /// 
    /// </summary>
    public class ActorProvider
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);
        private readonly TimeSpan _timeout = DEFAULT_TIMEOUT;

        public ActorProvider() { }

        public ActorProvider(TimeSpan timeout) 
        {
            _timeout = timeout;
        }

        public IActorRef? TryGetExistentActor(ActorSystem deployerSystem, Address systemAddress, string actorName)
        {
            try
            {
                var address = systemAddress.ToString() + "/user/" + actorName;
                return deployerSystem.ActorSelection(address).ResolveOne(_timeout).Result;

            } catch (ActorNotFoundException)
            {
                return null;
            } catch (AggregateException)
            {
                return null;
            }
        }

        public static IActorRef SpawnLocally(ActorSystem localActorSystem, Props actorProps, string actorName)
        {
            return localActorSystem.ActorOf(actorProps, actorName);
        }

        public static IActorRef SpawnRemote(ActorSystem deployerSystem, Address remoteAddress, Props actorProps, string actorName)
        {
            return deployerSystem.ActorOf(
                actorProps.WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))),
                actorName 
                );
        }
    }
}
