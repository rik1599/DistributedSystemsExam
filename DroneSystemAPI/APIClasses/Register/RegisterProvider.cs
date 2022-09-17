using Actors;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Utils;
using Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Register
{
    

    /// <summary>
    /// Strumento per il reperimento o per lo spawn di un registro dei nodi.
    /// </summary>
    public class RegisterProvider
    {
        private ActorSystem _localSystem;

        private DroneSystemConfig _config = new DroneSystemConfig();
       
        public RegisterProvider(ActorSystem localSystem)
        {
            _localSystem = localSystem; 
        }

        public RegisterProvider(ActorSystem localSystem, DroneSystemConfig config) : this(localSystem)
        {
            _config = config;
        }

        /// <summary>
        /// Prova a ottenere un registro esistente.
        /// </summary>
        /// <param name="host">host dove cercare il registro</param>
        /// <returns></returns>
        public RegisterAPI? TryConnectToExistent(Host host)
        {
            ActorProvider actorProvider = new ActorProvider();
            var actorRef = actorProvider.TryGetExistentActor(
                _localSystem, 
                Address.Parse(host.GetSystemAddress(_config.RegisterSystemName)),
                _config.RegisterActorName);

            return (actorRef is null) ? null : new RegisterAPI(actorRef);
        }

        /// <summary>
        /// Crea localmente (nel sistema indicato come locale) un registro
        /// </summary>
        /// <returns></returns>
        public RegisterAPI SpawnHere()
        {
            var actorRef = ActorProvider.SpawnLocally(
                _localSystem, 
                DronesRepositoryActor.Props(), 
                _config.RegisterActorName);

            return new RegisterAPI(actorRef);
        }

        /// <summary>
        /// Spawna su un sistema remoto il registro
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public RegisterAPI SpawnRemote(Host host)
        {
            var actorRef = ActorProvider.SpawnRemote(
                _localSystem, 
                Address.Parse(host.GetSystemAddress(_config.RegisterSystemName)), 
                DronesRepositoryActor.Props(), 
                _config.RegisterActorName);

            return new RegisterAPI(actorRef);
        }
    }
}
