using Actors;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Configuration;

namespace DroneSystemAPI.APIClasses.Utils
{
    public static class ActorSystemFactory
    {
        /// <summary>
        /// Crea unn nuovo actor system, inizializzato con l'attore spawner
        /// </summary>
        /// <param name="deployPointDetails">Indicazioni sulla locazione</param>
        /// <param name="assignedPort">porta reale che viene assegnata</param>
        /// <returns></returns>
        public static ActorSystem Create(DeployPointDetails deployPointDetails, out int assignedPort)
        {
            // todo: verifica l'input

            
            // creo l'AS
            // [todo: studiare che eccezioni lancia]
            ActorSystemImpl? system = ActorSystem.Create(
                    deployPointDetails.ActorSystemName,
                    ConfigurationFactory.ParseString(_makeHookon(deployPointDetails.Host))
                    ) as ActorSystemImpl;

            // estraggo la porta reale che è stata assegnata
            assignedPort = system!.LookupRoot.Provider.DefaultAddress.Port!.Value;

            // creo l'attore spawner
            system.ActorOf(Props.Create(() => new SpawnerActor()), "spawner");

            return system;
        }

        private static string _makeHookon(Host host)
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
}
