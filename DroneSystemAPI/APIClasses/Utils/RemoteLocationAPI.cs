using Actors;
using Akka.Actor;

namespace DroneSystemAPI.APIClasses.Utils
{
    /// <summary>
    /// Strumento unico per:
    /// - reperire facilmente gli indirizzi di attori collocati su sistemi noti
    /// - spawnare attori (localmente o su sistemi remoti avviati)
    /// 
    /// </summary>
    public class RemoteLocationAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Actor system locale utilizzato per interfacciarsi 
        /// con Akka.NET
        /// </summary>
        private ActorSystem _interfaceActorSystem;

        public DeployPointDetails DeployPointDetails { get; }

        public RemoteLocationAPI(ActorSystem interfaceActorSystem, DeployPointDetails deployPointDetails) 
        {
            _interfaceActorSystem = interfaceActorSystem;
            DeployPointDetails = deployPointDetails;
            _timeout = DEFAULT_TIMEOUT;
        }

        /// <summary>
        /// Ottieni il riferimento di un attore che si trova su
        /// questa locazione remota.
        /// </summary>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public IActorRef? GetActorRef(string actorName)
        {
            try
            {
                // var address = $"{systemAddress}/user/spawner/{actorName}";
                var address = DeployPointDetails.SpawnerAddress() + "/" + actorName;
                return _interfaceActorSystem.ActorSelection(address).ResolveOne(_timeout).Result;
            } catch (ActorNotFoundException)
            {
                return null;
            } catch (AggregateException)
            {
                return null;
            }

            // TODO: valutare se vale la pena propagare all'esterno
            //      le eccezioni per dare un idea del tipo di errore
        }

        /// <summary>
        /// Spawna un attore sulla locazione remota (attraverso
        /// il meccanismo dello spawner).
        /// </summary>
        /// <param name="actorProps"></param>
        /// <param name="actorName"></param>
        /// <returns></returns>
        public IActorRef? SpawnActor(Props actorProps, string actorName)
        {
            try
            {
                IActorRef remoteSpawner = _interfaceActorSystem
                        .ActorSelection(DeployPointDetails.SpawnerAddress())
                        .ResolveOne(_timeout).Result;

                var result = remoteSpawner.Ask(
                    new SpawnActorRequest(actorProps, actorName)).Result;

                if (result is IActorRef @ref) 
                    return @ref;
                else 
                    return null;
            }
            catch (Exception)
            {
                // TODO: gestire meglio eccezioni (valutare di non gestirle)
                return null;
            }
        }

        /// <summary>
        /// Verifica che la locazione remota sia raggiungibile
        /// e inizializzata.
        /// </summary>
        /// <returns></returns>
        public bool Verify()
        {
            try
            {
                IActorRef remoteSpawner = _interfaceActorSystem
                        .ActorSelection(DeployPointDetails.SpawnerAddress())
                        .ResolveOne(_timeout).Result;

                var res = remoteSpawner.Ask(new SpawnActorTestMessage()).Result;
                if (res is bool)
                    return (bool) res;
                else return false;
            }
            catch (Exception)
            {
                // TODO: gestire meglio eccezioni (valutare di non gestirle)
                return false;
            }
        }

        public static IActorRef? SpawnLocally(ActorSystem localActorSystem, Props actorProps, string actorName)
        {
            try
            {
                return localActorSystem.ActorOf(actorProps, actorName);
            }
            catch (InvalidActorNameException)
            {
                return null;
            }
        }
    }
}
