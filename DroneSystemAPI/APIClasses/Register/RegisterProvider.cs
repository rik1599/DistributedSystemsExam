using Actors;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Utils;

namespace DroneSystemAPI.APIClasses.Register
{
    /// <summary>
    /// Strumento per il reperimento o per lo spawn di un registro dei nodi.
    /// </summary>
    public class RepositoryProvider
    {
        private readonly ActorSystem _localSystem;

        private readonly DroneSystemConfig _config = new();
       
        public RepositoryProvider(ActorSystem localSystem)
        {
            _localSystem = localSystem; 
        }

        public RepositoryProvider(ActorSystem localSystem, DroneSystemConfig config) : this(localSystem)
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
            ActorProvider actorProvider = new ();
            var actorRef = actorProvider.TryGetExistentActor(
                _localSystem, 
                Address.Parse(host.GetSystemAddress(_config.RegisterSystemName)),
                _config.RegisterActorName);

            return (actorRef is null) ? null : new RepositoryAPI(actorRef);
        }

        /// <summary>
        /// Crea localmente (nel sistema indicato come locale) un registro
        /// </summary>
        /// <returns></returns>
        public RepositoryAPI SpawnHere()
        {
            var actorRef = ActorProvider.SpawnLocally(
                _localSystem, 
                DronesRepositoryActor.Props(), 
                _config.RegisterActorName);

            return new RepositoryAPI(actorRef);
        }

        /// <summary>
        /// Spawna su un sistema remoto il registro
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public RepositoryAPI SpawnRemote(Host host)
        {
            var actorRef = ActorProvider.SpawnRemote(
                _localSystem, 
                Address.Parse(host.GetSystemAddress(_config.RegisterSystemName)), 
                DronesRepositoryActor.Props(), 
                _config.RegisterActorName);

            return new RepositoryAPI(actorRef);
        }
    }
}
