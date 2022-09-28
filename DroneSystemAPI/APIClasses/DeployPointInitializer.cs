using Actors;
using Akka.Actor;
using Akka.Actor.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses
{
    public class DeployPointInitializer
    {
        public Host ThisHost { get; }
        public ActorSystem ThisActorSystem { get; }

        private IActorRef? _spawnerActor;

        public IActorRef? SpawnerActor { get { return _spawnerActor; } }

        public DeployPointInitializer(ActorSystem thisActorSystem)
        {
            ThisActorSystem = thisActorSystem;
            ThisHost = _resolveHostFromActorSystem(thisActorSystem);
        }

        public DeployPointInitializer(DeployPointDetails deployPointDetails)
        {
            ThisActorSystem = ActorSystem.Create(
                deployPointDetails.ActorSystemName, 
                _resolveHookonFromHost(deployPointDetails.Host)
                );

            ThisHost = _resolveHostFromActorSystem(ThisActorSystem);
        }


        /// <summary>
        /// Inizializza questo actor system, assicurandoti 
        /// che ci sia un attore spawner
        /// </summary>
        public void Init()
        {
            _spawnerActor = ThisActorSystem.ActorOf(Props.Create(() => new SpawnerActor()), "spawner");
        }


        public bool IsInitialized()
        {
            if (_spawnerActor is null)
                return false;

            try
            {
                var res = _spawnerActor.Ask(new SpawnActorTestMessage(), new TimeSpan(0,0,5)).Result;
                if (res is bool)
                    return (bool) res;
                else 
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        private static Host _resolveHostFromActorSystem(ActorSystem actorSystem)
        {
            if (actorSystem is not ActorSystemImpl)
                throw new CannotSolveHostException($"Errore, non riesco a ricavare un host dall'ActorSystem:\n{actorSystem}");

            
            try
            {
                ActorSystemImpl systemImpl = (ActorSystemImpl)actorSystem;

                return new Host(
                    systemImpl.LookupRoot.Provider.DefaultAddress.Host,
                    systemImpl.LookupRoot.Provider.DefaultAddress.Port ?? -1
                    );
            }
            catch (Exception ex)
            {
                throw new CannotSolveHostException(
                    $"Errore, non riesco a ricavare un host dall'ActorSystem:\n{actorSystem}", 
                    ex);
            }
        }

        private static string _resolveHookonFromHost(Host host)
        {
            return @$"
akka {{
    loglevel = WARNING
    actor {{
        provider = remote
    }}
    remote {{
        dot-netty.tcp {{
            port = {host.Port}
            hostname = {host.HostName}
        }}
    }}
}}";
        }
    }

    public class CannotSolveHostException : Exception
    {
        public CannotSolveHostException(string? message) : base(message)
        {
        }

        public CannotSolveHostException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
