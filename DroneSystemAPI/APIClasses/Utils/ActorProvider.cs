using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static readonly TimeSpan DEFAULT_TIMEOUT = new TimeSpan(0, 0, 10);
        private readonly TimeSpan _timeout = DEFAULT_TIMEOUT;

        public ActorProvider(TimeSpan timeout) 
        {
            _timeout = timeout;
        }

        public IActorRef? TryGetExistentActor(ActorSystem deployerSystem, Address systemAddress, String actorName)
        {
            try
            {
                return deployerSystem.ActorSelection(systemAddress.System + "/user/" + actorName).ResolveOne(_timeout).Result;
            } catch (ActorNotFoundException e)
            {
                return null;
            }
        }

        public IActorRef SpawnLocally(ActorSystem localActorSystem, Props actorProps, String actorName)
        {
            return localActorSystem.ActorOf(actorProps, actorName);
        }

        public IActorRef SpawnRemote(ActorSystem deployerSystem, Address remoteAddress, Props actorProps, String actorName)
        {
            return deployerSystem.ActorOf(
                actorProps.WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))),
                actorName 
                );
        }
    }
}
