using Actors;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Utils;

namespace DroneSystemAPI.APIClasses.Repository
{
    /// <summary>
    /// Strumento per il reperimento o per lo spawn di un registro dei nodi.
    /// </summary>
    public class RepositoryProvider
    {
        private readonly ActorSystem _localSystem;

        private readonly SystemConfigs _config = SystemConfigs.RepositoryConfig;
       
        public RepositoryProvider(ActorSystem localSystem)
        {
            _localSystem = localSystem; 
        }

        public RepositoryProvider(ActorSystem localSystem, SystemConfigs config) : this(localSystem)
        {
            _config = config;
        }

        /// <summary>
        /// Prova a ottenere un registro esistente.
        /// </summary>
        /// <param name="host">host dove cercare il registro</param>
        /// <returns></returns>
        public RepositoryAPI? TryConnectToExistent(Host host)
        {
            var actorRef = new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .GetActorRef(_config.ActorName);

            return (actorRef is null) ? null : new RepositoryAPI(actorRef);
        }

        /// <summary>
        /// Crea localmente (nel sistema indicato come locale) un registro
        /// </summary>
        /// <returns></returns>
        public RepositoryAPI? SpawnHere()
        {
            var actorRef = RemoteLocationAPI.SpawnLocally(
                _localSystem,
                DronesRepositoryActor.Props(),
                _config.ActorName);
            return actorRef is null ? null : new RepositoryAPI(actorRef);
        }

        /// <summary>
        /// Spawna su un sistema remoto il registro
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public RepositoryAPI? SpawnRemote(Host host)
        {
            var actorRef = new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .SpawnActor(DronesRepositoryActor.Props(), _config.ActorName);

            return actorRef is null ? null : new RepositoryAPI(actorRef);
        }
    }
}
