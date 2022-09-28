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
        public IActorRef? TryConnectToExistent(Host host)
        {
            return new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .GetActorRef(_config.ActorName);
        }

        /// <summary>
        /// Spawna su un sistema remoto il registro
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public IActorRef? SpawnRemote(Host host)
        {
            return new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .SpawnActor(DronesRepositoryActor.Props(), _config.ActorName);
        }
    }
}
